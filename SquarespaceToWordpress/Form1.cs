using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SquarespaceToWordpress
{
    public partial class Form1 : Form
    {
        private static XDocument squarespaceBackupDoc;
        private static XDocument wordpressExportDoc;

        public Form1()
        {
            InitializeComponent();
            ResetForm();
        }

        public class Blog
        {
            public string Text { get; set; }
            public XElement Value { get; set; }

            public override string ToString()
            {
                return this.Text;
            }
        }

        // Have user specify file, then load blogs from it
        private void loadSquarespaceXmlButton_Click(object sender, EventArgs e)
        {
            if(openSquarespaceXmlDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    // Load XML from file
                    squarespaceBackupDoc = XDocument.Load(openSquarespaceXmlDialog.FileName);

                    // Find the blogs in the backup file
                    IEnumerable<XElement> blogs = from blog in squarespaceBackupDoc.Descendants("module")
                                                  where blog.Element("type").Value.Equals("2")
                                                  select blog;

                    blogSelectionBox.Items.Clear();
                    if (blogs.Count() > 0)
                    {
                        // Populate blogs dropdown
                        foreach (XElement blog in blogs)
                        {
                            Blog foundBlog = new Blog();
                            foundBlog.Value = blog;
                            foundBlog.Text = blog.Element("page-title").Value;
                            blogSelectionBox.Items.Add(foundBlog);
                        }
                        blogSelectionBox.Enabled = true;
                    }
                    else
                    {
                        blogSelectionBox.Items.Add("No blogs found");
                        blogSelectionBox.SelectedIndex = 0;
                    }
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        // Have user specify file name to save, generate file, save it
        private void exportWordpressButton_Click(object sender, EventArgs e)
        {
            saveWordpressExportFile.ShowDialog();

            if (!string.IsNullOrWhiteSpace(saveWordpressExportFile.FileName))
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    GenerateWordpressExportDoc();
                    wordpressExportDoc.Save(saveWordpressExportFile.FileName);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        // Some SquareSpace date is encoded, this decodes it
        private string DecodeElement(XElement toDecode)
        {
            if (toDecode.Attributes("encoding").Count() > 0)
            {
                // decode
                byte[] encodedData = System.Convert.FromBase64String(toDecode.Value);
                return System.Text.Encoding.UTF8.GetString(encodedData);
            }
            else
            {
                // not encoded
                return toDecode.Value;
            }
        }

        // Generate Wordpress export XML from the specified blog in the Squarespace file
        private void GenerateWordpressExportDoc()
        {
            wordpressExportDoc = new XDocument();
            XElement selectedBlog = ((Blog)blogSelectionBox.SelectedItem).Value;
            XNamespace excerpt = "http://wordpress.org/export/1.2/excerpt/";
            XNamespace content = "http://purl.org/rss/1.0/modules/content/";
            XNamespace wfw = "http://wellformedweb.org/CommentAPI/";
            XNamespace dc = "http://purl.org/dc/elements/1.1/";
            XNamespace wp = "http://wordpress.org/export/1.2/";
            string url = "http://" +
                         (from host in squarespaceBackupDoc.Element("squarespace-wireframe").Element("virtual-hosts").Elements()
                          where host.Attributes().Count() > 1
                          select host.Attribute("name").Value).First() + "/";
            Uri baseUri = new Uri(url);
            Uri blogUri = new Uri(baseUri, selectedBlog.Element("url-id").Value);
            IEnumerable<string> storageModules = from module in squarespaceBackupDoc.Element("squarespace-wireframe").Element("general-storage-module").Elements()
                                                 select module.Element("url-id").Value;

            // Nodes
            // -rss
            XElement rss = new XElement("rss",
                new XAttribute("version","2.0"),
                new XAttribute(XNamespace.Xmlns + "excerpt", excerpt),
                new XAttribute(XNamespace.Xmlns + "content", content),
                new XAttribute(XNamespace.Xmlns + "wfw", wfw),
                new XAttribute(XNamespace.Xmlns + "dc", dc),
                new XAttribute(XNamespace.Xmlns + "wp", wp));

            // --channel
            XElement channel = new XElement("channel");
            //<title>I&#039;ve Got Munchies</title>
            channel.Add(new XElement("title", squarespaceBackupDoc.Element("squarespace-wireframe")
                                                                  .Element("website")
                                                                  .Element("site-title").Value));
            //<link>http://www.somedomain.com/blog</link>
            channel.Add(new XElement("link", blogUri.ToString()));
            //<description>Just another WordPress site</description>
            channel.Add(new XElement("description", selectedBlog.Element("config")
                                                                .Element("journal-module-configuration")
                                                                .Element("rss-description").Value));
            //<pubDate>Mon, 31 Dec 2012 19:51:14 +0000</pubDate>
            channel.Add(new XElement("pubDate", DateTime.Now.ToString("ddd, dd MMM yyyy HH':'mm':'ss") + " +0000"));
            //<language>en-US</language>
            channel.Add(new XElement("language", squarespaceBackupDoc.Element("squarespace-wireframe")
                                                                     .Element("website")
                                                                     .Element("language").Value));
            //<wp:wxr_version>1.2</wp:wxr_version>
            channel.Add(new XElement(wp + "wxr_version", "1.2"));
            //<wp:base_site_url>http://www.somedomain.com/</wp:base_site_url>
            channel.Add(new XElement(wp + "base_site_url", baseUri.ToString()));
            //<wp:base_blog_url>http://www.somedomain.com/blog</wp:base_blog_url>
            channel.Add(new XElement(wp + "base_blog_url", blogUri.ToString()));

            #region Authors
            //<wp:author>
            //  <wp:author_id>2</wp:author_id>
            //  <wp:author_login>Bob</wp:author_login>
            //  <wp:author_email></wp:author_email>
            //  <wp:author_display_name><![CDATA[Bob]]></wp:author_display_name>
            //  <wp:author_first_name><![CDATA[]]></wp:author_first_name>
            //  <wp:author_last_name><![CDATA[]]></wp:author_last_name>
            //</wp:author>
            IEnumerable<XElement> entries = squarespaceBackupDoc.Element("squarespace-wireframe")
                                                                .Element("website-member-accounts").Elements();
            Dictionary<string, string> authors = new Dictionary<string, string>();
            foreach (XElement entry in entries)
            {
                XElement author = new XElement(wp + "author");
                authors.Add(entry.Element("id").Value, DecodeElement(entry.Element("display-name")));
                author.Add(new XElement(wp + "author_id", entry.Element("id").Value));
                author.Add(new XElement(wp + "author_login", DecodeElement(entry.Element("display-name"))));
                author.Add(new XElement(wp + "author_email", entry.Element("email").Value));
                author.Add(new XElement(wp + "author_display_name", new XCData(DecodeElement(entry.Element("display-name")))));
                author.Add(new XElement(wp + "author_first_name", new XCData(entry.Element("first-name").Value)));
                author.Add(new XElement(wp + "author_last_name", new XCData(entry.Element("last-name").Value)));
                channel.Add(author);
            }
            #endregion

            #region Categories
            //<wp:category>
            //  <wp:term_id>8</wp:term_id>
            //  <wp:category_nicename>cast-and-crew</wp:category_nicename>
            //  <wp:category_parent></wp:category_parent>
            //  <wp:cat_name><![CDATA[Cast and Crew]]></wp:cat_name>
            //</wp:category>
            entries = from entry in selectedBlog.Element("content").Elements("journal-category")
                      where entry.Element("is-tag").Value.Equals("false")
                      select entry;
            foreach (XElement entry in entries)
            {
                XElement category = new XElement(wp + "category");
                category.Add(new XElement(wp + "term_id", entry.Element("id").Value));
                category.Add(new XElement(wp + "category_nicename", entry.Element("url-id").Value));
                category.Add(new XElement(wp + "category_parent", string.Empty));
                category.Add(new XElement(wp + "cat_name", new XCData(DecodeElement(entry.Element("name")))));
                channel.Add(category);
            } 
            #endregion

            #region Tags
            //<wp:tag>
            //  <wp:term_id>14</wp:term_id>
            //  <wp:tag_slug>dunkin-donuts</wp:tag_slug>
            //  <wp:tag_name><![CDATA[dunkin donuts]]></wp:tag_name>
            //</wp:tag>
            entries = from entry in selectedBlog.Element("content").Elements("journal-category")
                      where entry.Element("is-tag").Value.Equals("true")
                      select entry;
            foreach (XElement entry in entries)
            {
                XElement tag = new XElement(wp + "tag");
                tag.Add(new XElement(wp + "term_id", entry.Element("id").Value));
                tag.Add(new XElement(wp + "tag_slug", entry.Element("url-id").Value));
                tag.Add(new XElement(wp + "tag_name", new XCData(DecodeElement(entry.Element("name")))));
                channel.Add(tag);
            } 
            #endregion

            //<generator>http://wordpress.org/?v=3.5</generator>
            channel.Add(new XElement("generator", "SquarespaceToWordpress"));

            #region Posts

            // ---item
            entries = selectedBlog.Element("content").Elements("journal-entry");
            foreach (XElement entry in entries)
            {
                XElement item = new XElement("item");
                //  <title>The Evolution of Nachos</title>
                item.Add(new XElement("title", DecodeElement(entry.Element("title"))));
                //  <link>http://www.somedomain.com/blog/2010/11/09/the-evolution-of-nachos.html</link>
                Uri postUri = new Uri(baseUri, selectedBlog.Element("url-id").Value + "/" + entry.Element("url-id").Value);
                item.Add(new XElement("link", postUri.ToString()));
                //  <pubDate>Tue, 09 Nov 2010 02:33:48 +0000</pubDate>
                item.Add(new XElement("pubDate", entry.Element("added-on").Value));
                //  <dc:creator>Sharon</dc:creator>
                item.Add(new XElement(dc + "creator", authors[entry.Element("registered-author-id").Value]));
                //  <guid isPermaLink="false">http://www.somedomain.com/blog/2010/11/09/the-evolution-of-nachos/</guid>
                item.Add(new XElement("guid", new XAttribute("isPermaLink", "true"), postUri.ToString()));
                //  <description></description>
                item.Add(new XElement("description"));
                //  <content:encoded><![CDATA[]]></content:encoded>
                string postContent = ScrubHtml(DecodeElement(entry.Element("body")));
                string excerptContent = ScrubHtml(DecodeElement(entry.Element("excerpt")));

                if (FixVideoEmbed(ref postContent) || FixVideoEmbed(ref excerptContent))
                {
                    item.Add(new XElement("category",
                                          new XAttribute("domain", "post_format"),
                                          new XAttribute("nicename", "post-format-video"),
                                          new XCData("Video")));
                }

                #region Processing Squarespace images

                Regex imageSearch = new Regex(@"(?:src=\"")([^\""]*)(?:\?__SQUARESPACE_CACHEVERSION=[0-9]*?\"")", RegexOptions.Singleline);
                Match file = imageSearch.Match(postContent);
                string[] dates = entry.Element("url-id").Value.Split(new char[] { '/' });
                string contentBaseUrl = (new Uri(baseUri,
                                                 string.Format("{0}/wp-content/uploads/{1}/{2}",
                                                               selectedBlog.Element("url-id").Value,
                                                               dates[0],
                                                               (dates[1].Length > 1) ? dates[1] : 0 + dates[1]))).ToString();

                while (file.Success)
                {
                    string modules = string.Join("|", storageModules);
                    Uri imageUrl;
                    if (file.Groups[1].Value.StartsWith("http://"))
                    {
                        string final = file.Groups[1].Value;
                        imageUrl = new Uri(final);
                    }
                    else
                    {
                        if (storageModules.Contains(file.Groups[1].Value.Split(new string[] { "..", "/" }, StringSplitOptions.RemoveEmptyEntries)[0]))
                        {
                            Regex storageRoot = new Regex("(^.*/?)(" + modules + ")/", RegexOptions.None);
                            Match storageMatch = storageRoot.Match(file.Groups[1].Value);
                            imageUrl = new Uri(baseUri, file.Groups[1].Value.Remove(0, storageMatch.Groups[0].Index));
                        }
                        else
                        {
                            // Catch all when the URL is some other form...
                            imageUrl = new Uri(new Uri(baseUri, storageModules.First()),
                                               file.Groups[1].Value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last());
                        }
                    }
                    string fileDash = imageUrl.ToString().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last().Replace(' ', '-');
                    Uri postUrl = new Uri(contentBaseUrl + "/" + fileDash);
                    string imageTitle = imageUrl.ToString().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last();

                    XElement attachment = new XElement("item");
                    //<title></title>
                    attachment.Add(new XElement("title", imageTitle));
                    //<link>http://www.somedomain.com/blog/?attachment_id=451</link>
                    attachment.Add(new XElement("link", postUrl.ToString()));
                    //<pubDate>Sat, 29 Dec 2012 22:10:39 +0000</pubDate>
                    attachment.Add(new XElement("pubDate", entry.Element("added-on").Value));
                    //<dc:creator>igmAdmin</dc:creator>
                    attachment.Add(new XElement(dc + "creator", authors[entry.Element("registered-author-id").Value]));
                    //<guid isPermaLink="false">http://www.somedomain.com/blog/wp-content/uploads/2010/10/Apple%20Logo%201.jpg</guid>
                    attachment.Add(new XElement("guid", new XAttribute("isPermaLink", "true"), postUrl.ToString()));
                    //<description></description>
                    //attachment.Add(new XElement("description", file.Groups[1].Value + "<br /> in " + entry.Element("url-id").Value));
                    attachment.Add(new XElement("description", string.Empty));
                    //<content:encoded><![CDATA[]]></content:encoded>
                    attachment.Add(new XElement(content + "encoded", new XCData(string.Empty)));
                    //<excerpt:encoded><![CDATA[]]></excerpt:encoded>
                    attachment.Add(new XElement(excerpt + "encoded", new XCData(string.Empty)));
                    //<wp:post_id>451</wp:post_id>
                    attachment.Add(new XElement(wp + "post_id", string.Empty));
                    //<wp:post_date>2012-12-29 22:10:39</wp:post_date>
                    attachment.Add(new XElement(wp + "post_date", entry.Element("added-on").Value));
                    //<wp:post_date_gmt>2012-12-29 22:10:39</wp:post_date_gmt>
                    attachment.Add(new XElement(wp + "post_date_gmt", entry.Element("added-on").Value));
                    //<wp:comment_status>open</wp:comment_status>
                    attachment.Add(new XElement(wp + "comment_status", "open"));
                    //<wp:ping_status>open</wp:ping_status>
                    attachment.Add(new XElement(wp + "ping_status", "open"));
                    //<wp:post_name>451</wp:post_name>
                    attachment.Add(new XElement(wp + "post_id", string.Empty));
                    //<wp:status>inherit</wp:status>
                    attachment.Add(new XElement(wp + "status", "inherit"));
                    //<wp:post_parent>0</wp:post_parent>
                    attachment.Add(new XElement(wp + "post_parent", entry.Element("id").Value));
                    //<wp:menu_order>0</wp:menu_order>
                    attachment.Add(new XElement(wp + "menu_order", "0"));
                    //<wp:post_type>attachment</wp:post_type>
                    attachment.Add(new XElement(wp + "post_type", "attachment"));
                    //<wp:post_password></wp:post_password>
                    attachment.Add(new XElement(wp + "post_password", string.Empty));
                    //<wp:is_sticky>0</wp:is_sticky>
                    attachment.Add(new XElement(wp + "is_sticky", "0"));
                    //<wp:attachment_url>http://www.somedomain.com/blog/wp-content/uploads/2010/10/Apple%20Logo%201.jpg</wp:attachment_url>
                    attachment.Add(new XElement(wp + "attachment_url", imageUrl.AbsoluteUri));
                    //<wp:postmeta>
                    //    <wp:meta_key>_wp_attached_file</wp:meta_key>
                    //    <wp:meta_value><![CDATA[2010/10/Apple%20Logo%201.jpg]]></wp:meta_value>
                    //</wp:postmeta>
                    XElement postmeta = new XElement(wp + "postmeta");
                    postmeta.Add(new XElement(wp + "meta_key", "_wp_attached_file"));
                    postmeta.Add(new XElement(wp + "meta_value", new XCData(string.Format("{1}/{2}/",
                                                                            selectedBlog.Element("url-id").Value, dates[0],
                                                                            (dates[1].Length > 1) ? dates[1] : 0 + dates[1]) + fileDash)));
                    attachment.Add(postmeta);
                    //<wp:postmeta>
                    //    <wp:meta_key>_wp_attachment_metadata</wp:meta_key>
                    //    <wp:meta_value><![CDATA[a:5:{s:5:"width";i:150;s:6:"height";i:110;s:4:"file";s:28:"2010/10/Apple%20Logo%201.jpg";s:5:"sizes";a:0:{}s:10:"image_meta";a:10:{s:8:"aperture";i:0;s:6:"credit";s:0:"";s:6:"camera";s:0:"";s:7:"caption";s:0:"";s:17:"created_timestamp";i:0;s:9:"copyright";s:0:"";s:12:"focal_length";i:0;s:3:"iso";i:0;s:13:"shutter_speed";i:0;s:5:"title";s:13:"IGM REVISED 2";}}]]></wp:meta_value>
                    //</wp:postmeta>
                    channel.Add(attachment);
                    //postContent = postContent.Remove(file.Groups[1].Index, file.Groups[0].Length - 6).Insert(file.Groups[1].Index, imageUrl.ToString());

                    postContent = postContent.Replace(file.Groups[0].Value, string.Format("src=\"{0}\"", postUrl.ToString()));
                    excerptContent = excerptContent.Replace(file.Groups[0].Value, string.Format("src=\"{0}\"", postUrl.ToString()));
                    file = imageSearch.Match(postContent);
                }

                #endregion

                #region Add references to post content
                if ((entry.Elements("reference-count").Count() == 1) &&
            Convert.ToInt32(entry.Element("reference-count").Value) > 0)
                {
                    XElement[] references = (from reference in selectedBlog.Element("content").Elements("journal-entry-reference")
                                             where reference.Element("journal-entry-id").Value.Equals(entry.Element("id").Value)
                                             select reference).ToArray();
                    string toAppend = "<p>[";
                    for (int i = 0; i < references.Length; i++)
                    {
                        toAppend += string.Format("<a href=\"{0}\">{1}</a>",
                                        references[i].Element("link").Value,
                                        references[i].Element("title").Value);
                        if (i < (references.Length - 1))
                        {
                            toAppend += ", ";
                        }
                    }
                    toAppend += "]</p>";
                    postContent += toAppend;
                } 
                #endregion
                item.Add(new XElement(content + "encoded", new XCData(postContent)));
                //  <excerpt:encoded><![CDATA[]]></excerpt:encoded>
                item.Add(new XElement(excerpt + "encoded", new XCData(excerptContent)));
                //  <wp:post_id>1240</wp:post_id>
                item.Add(new XElement(wp + "post_id", entry.Element("id").Value));
                //  <wp:post_date>2010-11-09 02:33:48</wp:post_date>
                item.Add(new XElement(wp + "post_date", entry.Element("added-on").Value));
                //  <wp:post_date_gmt>2010-11-09 02:33:48</wp:post_date_gmt>
                item.Add(new XElement(wp + "post_date_gmt", entry.Element("added-on").Value));
                //  <wp:comment_status>open</wp:comment_status>
                item.Add(new XElement(wp + "comment_status", Convert.ToBoolean(entry.Element("allow-comments").Value) ? "open" : "closed"));
                //  <wp:ping_status>open</wp:ping_status>
                item.Add(new XElement(wp + "ping_status", Convert.ToBoolean(entry.Element("allow-comments").Value) ? "open" : "closed"));
                //  <wp:post_name>the-evolution-of-nachos</wp:post_name>
                string page = entry.Element("url-id").Value.Trim().Split(new char[] { '/' }).Last();
                string postName = page.Remove(page.Length - 5);
                item.Add(new XElement(wp + "post_name", postName));
                //  <wp:status>publish</wp:status>
                item.Add(new XElement(wp + "status", Convert.ToBoolean(entry.Element("published").Value) ? "publish" : "draft"));
                //  <wp:post_parent>0</wp:post_parent>
                item.Add(new XElement(wp + "post_parent", "0"));
                //  <wp:menu_order>0</wp:menu_order>
                item.Add(new XElement(wp + "menu_order", "0"));
                //  <wp:post_type>post</wp:post_type>
                item.Add(new XElement(wp + "post_type", "post"));
                //  <wp:post_password></wp:post_password>
                item.Add(new XElement(wp + "post_password", string.Empty));
                //  <wp:is_sticky>0</wp:is_sticky>
                item.Add(new XElement(wp + "is_sticky", Convert.ToBoolean(entry.Element("sticky").Value) ? "1" : "0"));
                //  <category domain="category" nicename="food-finds"><![CDATA[Food Finds]]></category>
                //  <category domain="tag" nicename="newtag"><![CDATA[NewTag]]></category>
                IEnumerable<XElement> cats = from catLink in selectedBlog.Element("content").Elements("journal-category-association")
                                             where catLink.Element("journal-entry-id").Value.Equals(entry.Element("id").Value)
                                             select catLink;
                foreach (XElement cat in cats)
                {
                    XElement catEntry = (from catItem in selectedBlog.Element("content").Elements("journal-category")
                                         where catItem.Element("id").Value.Equals(cat.Element("journal-category-id").Value)
                                         select catItem).First();
                    item.Add(new XElement("category",
                                          new XAttribute("domain", Convert.ToBoolean(catEntry.Element("is-tag").Value) ? "tag" : "category"),
                                          new XAttribute("nicename", catEntry.Element("url-id").Value),
                                          new XCData(DecodeElement(catEntry.Element("name")))));
                }



                #region Comments for post

                if ((entry.Elements("comment-count").Count() == 1) &&
            Convert.ToInt32(entry.Element("comment-count").Value) > 0)
                {
                    IEnumerable<XElement> commentLinks = (from reference in selectedBlog.Element("content").Elements("journal-entry-comment")
                                                          where reference.Element("journal-entry-id").Value.Equals(entry.Element("id").Value)
                                                          select reference);
                    foreach (XElement link in commentLinks)
                    {
                        XElement commentEntry = (from commentInstance in selectedBlog.Element("content").Elements("comment")
                                                 where commentInstance.Element("id").Value.Equals(link.Element("comment-id").Value)
                                                 select commentInstance).First();
                        // This should be reworked if registered users had also been commenting
                        XElement commentAuthor = (from authorInstance in squarespaceBackupDoc.Element("squarespace-wireframe").Element("unregistered-authors").Elements()
                                                  where authorInstance.Element("id").Value.Equals(commentEntry.Element("unregistered-author-id").Value)
                                                  select authorInstance).First();
                        //  <wp:comment>
                        XElement wpComment = new XElement(wp + "comment");
                        //      <wp:comment_id>7</wp:comment_id>
                        wpComment.Add(new XElement(wp + "comment_id", commentEntry.Element("id").Value));
                        //      <wp:comment_author><![CDATA[Jack]]></wp:comment_author>
                        wpComment.Add(new XElement(wp + "comment_author", new XCData(commentAuthor.Element("name").Value)));
                        //      <wp:comment_author_email></wp:comment_author_email>
                        wpComment.Add(new XElement(wp + "comment_author_email", commentAuthor.Element("email").Value));
                        //      <wp:comment_author_url></wp:comment_author_url>
                        wpComment.Add(new XElement(wp + "comment_author_url", commentAuthor.Element("url").Value));
                        //      <wp:comment_author_IP>87.117.195.7</wp:comment_author_IP>
                        wpComment.Add(new XElement(wp + "comment_author_IP", commentAuthor.Element("ip").Value));
                        //      <wp:comment_date>2010-12-16 17:05:12</wp:comment_date>
                        wpComment.Add(new XElement(wp + "comment_date", commentEntry.Element("added-on").Value));
                        //      <wp:comment_date_gmt>2010-12-16 17:05:12</wp:comment_date_gmt>
                        wpComment.Add(new XElement(wp + "comment_date_gmt", commentEntry.Element("added-on").Value));
                        //      <wp:comment_content><![CDATA[]]></wp:comment_content>
                        string comment = ScrubHtml(DecodeElement(entry.Element("body")));
                        FixVideoEmbed(ref comment);
                        wpComment.Add(new XElement(wp + "comment_content", new XCData(comment)));
                        //      <wp:comment_approved>1</wp:comment_approved>
                        wpComment.Add(new XElement(wp + "comment_approved", Convert.ToBoolean(commentEntry.Element("approved").Value) ? "1" : "0"));
                        //      <wp:comment_type></wp:comment_type>
                        wpComment.Add(new XElement(wp + "comment_type", string.Empty));
                        //      <wp:comment_parent>0</wp:comment_parent>
                        wpComment.Add(new XElement(wp + "comment_parent", "0"));
                        //      <wp:comment_user_id>0</wp:comment_user_id>
                        wpComment.Add(new XElement(wp + "comment_user_id", commentAuthor.Element("id").Value));
                        //  </wp:comment>
                        item.Add(wpComment);
                    }
                }

                #endregion

                channel.Add(item);
            }

            #endregion

            rss.Add(channel);
            wordpressExportDoc.Add(rss);
        }

        // Remove Squarespace junk
        //  - styling (i.e. classes)
        private string ScrubHtml(string p)
        {
            Regex classAttrib = new Regex(@" class=""([^""\\]|\\.)*""");
            return classAttrib.Replace(p, string.Empty);
        }

        /// <summary>
        /// Converts Vimeo, YouTube, Archive.org, and Funny or Die video embeds into a cleaner format
        /// </summary>
        /// <param name="p">Text to process</param>
        /// <returns>Whether the text contained a video embed</returns>
        private bool FixVideoEmbed(ref string p)
        {
            bool containsEmbed = false;
            Regex youtubeSearch = new Regex(@"(?:<iframe .*src=""http://(?:www.)?youtube.com/embed/([^""/]*)""[^<]*></iframe>|" +
                                            @"<object .*<embed .*src=""http://(?:www.)?youtube.com/v/([^""/]*)"".*</object>)");
            Regex vimeoSearch = new Regex(@"(?:<iframe .*src=""http://player.vimeo.com/video/([0-9]*[^""]*)""[^<]*></iframe>|" +
                                          @"<object .*<embed .*src=""http://(?:www.)?vimeo.com/[^=""]*clip_id=([0-9]*[^""]*)"".*</object>)");
            Regex archiveSearch = new Regex(@"(?:<object .*)?<embed .*[""']http://www.archive.org/download/([^'""/]*)/.*</embed>(?:.*</object>)?");
            Regex fodSearch = new Regex(@"(?:<iframe .*src=""http://(?:www.)?funnyordie.com/embed/([^""/]*)""[^<]*></iframe>|" +
                                        @"<object .*http://player.ordienetworks.com.*<embed .*key=([^""]*)"".*</object>)");

            string amp = "&amp;";
            string archiveFormat = @"<iframe src=""http://archive.org/embed/{0}"" width=""100%"" height=""382"" frameborder=""0"" allowFullScreen></iframe>";
            string youtubeFormat = Environment.NewLine + "[embed]http://www.youtube.com/watch?v={0}[/embed]" + Environment.NewLine;
            string youtubeTimeFormat = amp + "t={0}m{1}s";
            string youtubeRemoveCode = amp + "rel=0";
            string vimeoFormat = Environment.NewLine + "[embed]http://vimeo.com/{0}[/embed]" + Environment.NewLine;
            string fodFormat = Environment.NewLine + "[embed]http://www.funnyordie.com/videos/{0}[/embed]" + Environment.NewLine;

            Match search = youtubeSearch.Match(p);

            while (search.Success)
            {
                string embedCode;
                string[] queryParams = string.IsNullOrWhiteSpace(search.Groups[1].Value) ? 
                                                                    search.Groups[2].Value.Split(new string[] { "?", amp }, StringSplitOptions.RemoveEmptyEntries) :
                                                                    search.Groups[1].Value.Split(new string[] { "?", amp }, StringSplitOptions.RemoveEmptyEntries);
                string queryString = queryParams[0];
                
                if (queryParams.Length > 1) // find existing start time references
                {
                    for (int i = 1; i < queryParams.Length; i++)
                    {
                        string[] param = queryParams[i].Split(new char[] { '=' });
                        if (param[0].Equals("start", StringComparison.OrdinalIgnoreCase))
                        {
                            int seconds = Convert.ToInt32(param[1]);
                            queryString += string.Format(youtubeTimeFormat, seconds/60, seconds%60);
                            break;
                        }
                        else if (param[0].Equals("t", StringComparison.OrdinalIgnoreCase))
                        {
                            queryString += amp + queryParams[i];
                            break;
                        }
                    }
                }

                queryString += youtubeRemoveCode;
                embedCode = string.Format(youtubeFormat, queryString);
                p = p.Remove(search.Index, search.Length);
                p = p.Insert(search.Index, embedCode);
                containsEmbed = true;

                search = youtubeSearch.Match(p, search.Index + embedCode.Length);
            }

            search = vimeoSearch.Match(p);

            while (search.Success)
            {
                string embedCode;
                string queryString = string.IsNullOrWhiteSpace(search.Groups[1].Value) ?
                                                                    search.Groups[2].Value :
                                                                    search.Groups[1].Value;
                
                queryString = queryString.Replace("&amp;server=vimeo.com", string.Empty);
                queryString = queryString.Replace("&amp;fullscreen=1", string.Empty);
                embedCode = string.Format(vimeoFormat, queryString);
                p = p.Remove(search.Index, search.Length);
                p = p.Insert(search.Index, embedCode);
                containsEmbed = true;

                search = vimeoSearch.Match(p, search.Index + embedCode.Length);
            }

            search = archiveSearch.Match(p);

            while (search.Success)
            {
                string embedCode;
                string queryString = search.Groups[1].Value;

                embedCode = string.Format(archiveFormat, queryString);
                p = p.Remove(search.Index, search.Length);
                p = p.Insert(search.Index, embedCode);
                containsEmbed = true;

                search = archiveSearch.Match(p, search.Index + embedCode.Length);
            }

            search = fodSearch.Match(p);

            while (search.Success)
            {
                string embedCode;
                string queryString = string.IsNullOrWhiteSpace(search.Groups[1].Value) ?
                                                                    search.Groups[2].Value :
                                                                    search.Groups[1].Value;

                embedCode = string.Format(fodFormat, queryString);
                p = p.Remove(search.Index, search.Length);
                p = p.Insert(search.Index, embedCode);
                containsEmbed = true;

                search = fodSearch.Match(p, search.Index + embedCode.Length);
            }

            return containsEmbed;
        }

        private void ResetBlogInfo()
        {
            entriesTextBox.Text = string.Empty;
            referencesTextBox.Text = string.Empty;
            commentsTextBox.Text = string.Empty;
            categoriesTextBox.Text = string.Empty;
            tagsTextBox.Text = string.Empty;
            exportWordpressButton.Enabled = false;
        }

        private void ResetForm()
        {
            blogSelectionBox.Items.Clear();
            blogSelectionBox.Enabled = false;
            ResetBlogInfo();
        }

        private void blogSelectionBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (blogSelectionBox.SelectedItem.GetType().Equals(typeof(Blog)))
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    // Get details about selected blog
                    XElement selectedBlog = ((Blog)blogSelectionBox.SelectedItem).Value;

                    IEnumerable<XElement> entries = selectedBlog.Element("content").Elements("journal-entry");
                    entriesTextBox.Text = entries.Count().ToString();

                    entries = selectedBlog.Element("content").Elements("journal-entry-reference");
                    referencesTextBox.Text = entries.Count().ToString();

                    entries = selectedBlog.Element("content").Elements("comment");
                    commentsTextBox.Text = entries.Count().ToString();

                    entries = from entry in selectedBlog.Element("content").Elements("journal-category")
                              where entry.Element("is-tag").Value.Equals("false")
                              select entry;
                    categoriesTextBox.Text = entries.Count().ToString();

                    entries = from entry in selectedBlog.Element("content").Elements("journal-category")
                              where entry.Element("is-tag").Value.Equals("true")
                              select entry;
                    tagsTextBox.Text = entries.Count().ToString();

                    exportWordpressButton.Enabled = true;
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }
    }
}
