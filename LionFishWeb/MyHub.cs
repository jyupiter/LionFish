using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LionFishWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

using Microsoft.AspNet.SignalR;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Web;

namespace LionFishWeb
{
    public class MyHub : Hub
    {

        public static readonly Regex forbiddenTags = new Regex("^(script|object|embed|link|style|form|input)$");
        public static readonly Regex allowedTags = new Regex("^(b|p|i|s|a|img|table|thead|tbody|tfoot|tr|th|td|dd|dl|dt|em|h1|h2|h3|h4|h5|h6|li|ul|ol|span|div|strike|strong|" +
                "sub|sup|pre|del|code|blockquote|strike|kbd|br|hr|area|map|object|embed|param|link|form|small|big)$");

        private static readonly Regex commentPattern = new Regex("<!--.*");  // <!--.........> 
        private static readonly Regex tagStartPattern = new Regex("<(?i)(\\w+\\b)\\s*(.*)/?>$");  // <tag ....props.....> 
        private static readonly Regex tagClosePattern = new Regex("</(?i)(\\w+\\b)\\s*>$");  // </tag .........> 

        private static readonly Regex standAloneTags = new Regex("^(img|br|hr)$");
        private static readonly Regex selfClosed = new Regex("<.+/>");

        private static readonly Regex attributesPattern = new Regex("(\\w*)\\s*=\\s*\"([^\"]*)\"");  // prop="...." 
        private static readonly Regex stylePattern = new Regex("([^\\s^:]+)\\s*:\\s*([^;]+);?");  // color:red; 

        private static readonly Regex urlStylePattern = new Regex("(?i).*\\b\\s*url\\s*\\(['\"]([^)]*)['\"]\\)");  // url('....')" 

        public static readonly Regex forbiddenStylePattern = new Regex("(?:(expression|eval|javascript))\\s*\\(");

        public static SanitizeResult sanitizer(String html)
        {
            return sanitizer(html, allowedTags, forbiddenTags);
        }

        public static SanitizeResult sanitizer(String html, Regex allowedTags, Regex forbiddenTags)
        {
            SanitizeResult ret = new SanitizeResult();
            Stack<String> openTags = new Stack<string>();

            if (String.IsNullOrEmpty(html))
                return ret;

            List<String> tokens = tokenize(html);

            // -------------------   LOOP for every token -------------------------- 
            for (int i = 0; i < tokens.Count; i++)
            {
                String token = tokens[i];
                bool isAcceptedToken = false;

                Match startMatcher = tagStartPattern.Match(token);
                Match endMatcher = tagClosePattern.Match(token);

                //--------------------------------------------------------------------------------  COMMENT    <!-- ......... --> 
                if (commentPattern.Match(token).Success)
                {
                    ret.val = ret.val + token + (token.EndsWith("-->") ? "" : "-->");
                    ret.invalidTags.Add(token + (token.EndsWith("-->") ? "" : "-->"));
                    continue;

                    //--------------------------------------------------------------------------------  OPEN TAG    <tag .........> 
                }
                else if (startMatcher.Success)
                {

                    //tag name extraction 
                    String tag = startMatcher.Groups[1].Value.ToLower();

                    //-----------------------------------------------------  FORBIDDEN TAG   <script .........> 
                    if (forbiddenTags.Match(tag).Success)
                    {
                        ret.invalidTags.Add("<" + tag + ">");
                        continue;

                        // --------------------------------------------------  WELL KNOWN TAG 
                    }
                    else if (allowedTags.Match(tag).Success)
                    {

                        String cleanToken = "<" + tag;
                        String tokenBody = startMatcher.Groups[2].Value;

                        //first test table consistency 
                        //table tbody tfoot thead th tr td 
                        if ("thead".Equals(tag) || "tbody".Equals(tag) || "tfoot".Equals(tag) || "tr".Equals(tag))
                        {
                            if (openTags.Select(t => t == "table").Count() <= 0)
                            {
                                ret.invalidTags.Add("<" + tag + ">");
                                continue;
                            }
                        }
                        else if ("td".Equals(tag) || "th".Equals(tag))
                        {
                            if (openTags.Count(t => t == "tr") <= 0)
                            {
                                ret.invalidTags.Add("<" + tag + ">");
                                continue;
                            }
                        }

                        // then test properties 
                        //Match attributes = attributesPattern.Match(tokenBody);
                        var attributes = attributesPattern.Matches(tokenBody);

                        bool foundURL = false; // URL flag

                        foreach (Match attribute in attributes)
                        //while (attributes.find())
                        {
                            String attr = attribute.Groups[1].Value.ToLower();
                            String val = attribute.Groups[2].Value;

                            // we will accept href in case of <A> 
                            if ("a".Equals(tag) && "href".Equals(attr))
                            {    // <a href="......">


                                try
                                {
                                    var url = new Uri(val);

                                    if (url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps || url.Scheme == Uri.UriSchemeMailto)
                                    {
                                        foundURL = true;
                                    }
                                    else
                                    {
                                        ret.invalidTags.Add(attr + " " + val);
                                        val = "";
                                    }
                                }
                                catch
                                {
                                    ret.invalidTags.Add(attr + " " + val);
                                    val = "";
                                }
                            }
                            else if ((tag == "img" || tag == "embed") && "src".Equals(attr))
                            { // <img src="......"> 
                                try
                                {
                                    var url = new Uri(val);

                                    if (url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps)
                                    {
                                        foundURL = true;
                                    }
                                    else
                                    {
                                        ret.invalidTags.Add(attr + " " + val);
                                        val = "";
                                    }
                                }
                                catch
                                {
                                    ret.invalidTags.Add(attr + " " + val);
                                    val = "";
                                }

                            }
                            else if ("href".Equals(attr) || "src".Equals(attr))
                            { // <tag src/href="......">   skipped 
                                ret.invalidTags.Add(tag + " " + attr + " " + val);
                                continue;

                            }
                            else if (attr == "width" || attr == "height")
                            { // <tag width/height="......">
                                Regex r = new Regex("\\d+%|\\d+$");
                                if (!r.Match(val.ToLower()).Success)
                                { // test numeric values 
                                    ret.invalidTags.Add(tag + " " + attr + " " + val);
                                    continue;
                                }

                            }
                            else if ("style".Equals(attr))
                            { // <tag style="......"> 

                                // then test properties 
                                var styles = stylePattern.Matches(val);
                                String cleanStyle = "";

                                foreach (Match style in styles)
                                //while (styles.find())
                                {
                                    String styleName = style.Groups[1].Value.ToLower();
                                    String styleValue = style.Groups[2].Value;

                                    // suppress invalid styles values 
                                    if (forbiddenStylePattern.Match(styleValue).Success)
                                    {
                                        ret.invalidTags.Add(tag + " " + attr + " " + styleValue);
                                        continue;
                                    }

                                    // check if valid url 
                                    Match urlStyleMatcher = urlStylePattern.Match(styleValue);
                                    if (urlStyleMatcher.Success)
                                    {
                                        try
                                        {
                                            String url = urlStyleMatcher.Groups[1].Value;
                                            var uri = new Uri(url);

                                            if (!(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                                            {
                                                ret.invalidTags.Add(tag + " " + attr + " " + styleValue);
                                                continue;
                                            }
                                        }
                                        catch
                                        {
                                            ret.invalidTags.Add(tag + " " + attr + " " + styleValue);
                                            continue;
                                        }

                                    }

                                    cleanStyle = cleanStyle + styleName + ":" + encode(styleValue) + ";";

                                }
                                val = cleanStyle;

                            }
                            else if (attr.StartsWith("on"))
                            {  // skip all javascript events 
                                ret.invalidTags.Add(tag + " " + attr + " " + val);
                                continue;

                            }
                            else
                            {  // by default encode all properies 
                                val = encode(val);
                            }

                            cleanToken = cleanToken + " " + attr + "=\"" + val + "\"";
                        }
                        cleanToken = cleanToken + ">";

                        isAcceptedToken = true;

                        // for <img> and <a>
                        if ((tag == "a" || tag == "img" || tag == "embed") && !foundURL)
                        {
                            isAcceptedToken = false;
                            cleanToken = "";
                        }

                        token = cleanToken;

                        // push the tag if require closure and it is accepted (otherwise is encoded) 
                        if (isAcceptedToken && !(standAloneTags.Match(tag).Success || selfClosed.Match(tag).Success))
                            openTags.Push(tag);

                        // --------------------------------------------------------------------------------  UNKNOWN TAG 
                    }
                    else
                    {
                        ret.invalidTags.Add(token);
                        ret.val = ret.val + token;
                        continue;

                    }

                    // --------------------------------------------------------------------------------  CLOSE TAG </tag> 
                }
                else if (endMatcher.Success)
                {
                    String tag = endMatcher.Groups[1].Value.ToLower();

                    //is self closing 
                    if (selfClosed.Match(tag).Success)
                    {
                        ret.invalidTags.Add(token);
                        continue;
                    }
                    if (forbiddenTags.Match(tag).Success)
                    {
                        ret.invalidTags.Add("/" + tag);
                        continue;
                    }
                    if (!allowedTags.Match(tag).Success)
                    {
                        ret.invalidTags.Add(token);
                        ret.val = ret.val + token;
                        continue;
                    }
                    else
                    {


                        String cleanToken = "";

                        // check tag position in the stack 

                        int pos = -1;
                        bool found = false;

                        foreach (var item in openTags)
                        {
                            pos++;
                            if (item == tag)
                            {
                                found = true;
                                break;
                            }
                        }

                        // if found on top ok 
                        if (found)
                        {
                            for (int k = 0; k <= pos; k++)
                            {
                                //pop all elements before tag and close it 
                                String poppedTag = openTags.Pop();
                                cleanToken = cleanToken + "</" + poppedTag + ">";
                                isAcceptedToken = true;
                            }
                        }

                        token = cleanToken;
                    }

                }

                ret.val = ret.val + token;

                if (isAcceptedToken)
                {
                    ret.html = ret.html + token;
                    //ret.text = ret.text + " "; 
                }
                else
                {
                    String sanToken = htmlEncodeApexesAndTags(token);
                    ret.html = ret.html + sanToken;
                    ret.text = ret.text + htmlEncodeApexesAndTags(removeLineFeed(token));
                }


            }

            // must close remaining tags 
            while (openTags.Count() > 0)
            {
                //pop all elements before tag and close it 
                String poppedTag = openTags.Pop();
                ret.html = ret.html + "</" + poppedTag + ">";
                ret.val = ret.val + "</" + poppedTag + ">";
            }

            //set boolean value 
            ret.isValid = ret.invalidTags.Count == 0;

            return ret;
        }

        /** 
         * Splits html tag and tag content <......>. 
         * 
         * @param html 
         * @return a list of token 
         */
        private static List<String> tokenize(String html)
        {
            //ArrayList tokens = new ArrayList();
            List<String> tokens = new List<string>();
            int pos = 0;
            String token = "";
            int len = html.Length;
            while (pos < len)
            {
                char c = html[pos];

                // BBB String ahead = html.Substring(pos, pos > len - 4 ? len : pos + 4);
                String ahead = html.Substring(pos, pos > len - 4 ? len - pos : 4);

                //a comment is starting 
                if ("<!--".Equals(ahead))
                {
                    //store the current token 
                    if (token.Length > 0)
                        tokens.Add(token);

                    //clear the token 
                    token = "";

                    // serch the end of <......> 
                    int end = moveToMarkerEnd(pos, "-->", html);

                    // BBB tokens.Add(html.Substring(pos, end));
                    tokens.Add(html.Substring(pos, end - pos));
                    pos = end;


                    // a new "<" token is starting 
                }
                else if ('<' == c)
                {

                    //store the current token 
                    if (token.Length > 0)
                        tokens.Add(token);

                    //clear the token 
                    token = "";

                    // serch the end of <......> 
                    int end = moveToMarkerEnd(pos, ">", html);
                    // BBB tokens.Add(html.Substring(pos, end));
                    tokens.Add(html.Substring(pos, end - pos));
                    pos = end;

                }
                else
                {
                    token = token + c;
                    pos++;
                }

            }

            //store the last token 
            if (token.Length > 0)
                tokens.Add(token);

            return tokens;
        }


        private static int moveToMarkerEnd(int pos, String marker, String s)
        {
            int i = s.IndexOf(marker, pos);
            if (i > -1)
                pos = i + marker.Length;
            else
                pos = s.Length;
            return pos;
        }

        /** 
         * Contains the sanitizing results. 
         * html is the sanitized html encoded  ready to be printed. Unaccepted tags are encode, text inside tag is always encoded   MUST BE USED WHEN PRINTING HTML 
         * text is the text inside valid tags. Contains invalid tags encoded                                                        SHOULD BE USED TO PRINT EXCERPTS 
         * val  is the html source cleaned from unaccepted tags. It is not encoded:                                                 SHOULD BE USED IN SAVE ACTIONS 
         * isValid is true when every tag is accepted without forcing encoding 
         * invalidTags is the list of encoded-killed tags 
         */
        public class SanitizeResult
        {
            public String html = "";
            public String text = "";
            public String val = "";
            public bool isValid = true;
            public List<String> invalidTags = new List<string>();
        }

        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public static String encode(String s)
        {
            return convertLineFeedToBR(htmlEncodeApexesAndTags(s == null ? "" : s));
        }

        public static String htmlEncodeApexesAndTags(String source)
        {
            return htmlEncodeTag(htmlEncodeApexes(source));
        }

        public static String htmlEncodeApexes(String source)
        {
            /*if (source != null)
            {
                String result = replaceAllNoRegex(source, new String[] { "&", "\"", "'" }, new String[] { "&amp;", "&quot;", "&#39;" });
                return result;
            }
            else
                return null;*/
            if (source != null)
            {
                String result = replaceAllNoRegex(source, new String[] { "\"", "'" }, new String[] { "&quot;", "&#39;" });
                return result;
            }
            else
                return null;
        }

        public static String htmlEncodeTag(String source)
        {
            if (source != null)
            {
                String result = replaceAllNoRegex(source, new String[] { "<", ">" }, new String[] { "&lt;", "&gt;" });
                return result;
            }
            else
                return null;
        }


        public static String convertLineFeedToBR(String text)
        {
            if (text != null)
                return replaceAllNoRegex(text, new String[] { "\n", "\f", "\r" }, new String[] { "<br>", "<br>", " " });
            else
                return null;
        }

        public static String removeLineFeed(String text)
        {

            if (text != null)
                return replaceAllNoRegex(text, new String[] { "\n", "\f", "\r" }, new String[] { " ", " ", " " });
            else
                return null;
        }



        public static String replaceAllNoRegex(String source, String[] searches, String[] replaces)
        {
            int k;
            String tmp = source;
            for (k = 0; k < searches.Length; k++)
                tmp = replaceAllNoRegex(tmp, searches[k], replaces[k]);
            return tmp;
        }

        public static String replaceAllNoRegex(String source, String search, String replace)
        {
            StringBuilder buffer = new StringBuilder();
            if (source != null)
            {
                if (search.Length == 0)
                    return source;
                int oldPos, pos;
                for (oldPos = 0, pos = source.IndexOf(search, oldPos); pos != -1; oldPos = pos + search.Length, pos = source.IndexOf(search, oldPos))
                {
                    buffer.Append(source.Substring(oldPos, pos));
                    buffer.Append(replace);
                }
                if (oldPos < source.Length)
                    buffer.Append(source.Substring(oldPos));
            }
            return buffer.ToString();
        }
        // sad sad sad
        public async Task JoinRoom(string roomName)
        {

            await Groups.Add(Context.ConnectionId, roomName);
        }
        // same connection, just showing open groups/pm based on users.
        // this is so sad alexa play d͇̤̞̦͎͍̻͇͉̘̖ͪ̽̾̄̌̏̅̑ͧ͐̏͋͘ę͙̩̳̺̜̹̝̣̽ͬ́͂͡śͯ̎͒̄ͪ̊ͫ̃͆̄͊͛̓̓͋͏͏̛͔͉͇͖͇͡ͅpͭ̿̓̄̎̾̅͊̐҉̴̛̥̭̞̣̬͉̭͔̠̙̲̯͓a̷̶̢̡̜̻̝͖̥̠̯̮̗͎͓̪̰̯̭͔̗ͥͫ̌ͩ̓̎͐̐c̛͗ͬͯ͒̇̈́ͥͭ̑̆ͩͣ̓̄̐̓͜͜͏̭͚̯͇͔̗͟i͈̰͈̤̤̥̮͍͑̽̾̍̉ͣ̈́͆͗̇̊̽̿̂̚͟͝t̵ͣͩ̉̆̉̒ͬ́̂ͮ̏͑̇͐͒͑͏̴̨̟̫̜̟̪̠̬͎̘̦̻̻̺̦͖̹͓͎͞o̧̧̡̤̘̙̜̖͕̮̻͒ͨ͊̇ͫ͆͑ͪ͂̔ͨ͝
        public async Task LeaveRoom(string roomName)
        {
            await Groups.Remove(Context.ConnectionId, roomName);
        }



        //private string Sanitize(string toClean) {
        //    HashSet<char> removeChars = new HashSet<char>(" ?&^$#@!()+-,:;<>’\'-_*");
        //    StringBuilder result = new StringBuilder(toClean.Length);
        //    foreach (char c in toClean)
        //        if (!removeChars.Contains(c)) // prevent dirty chars
        //            result.Append(c);
        //    return result.ToString();
        //}

        public async Task JoinGroup()
        {
            //query for message for pertaining group (PM counted as user self group like linux or whatever)
            // var query1 = context.Messages.Where(s => s.ID == 123).FirstOrDefault<Message>();

            //fill stack with queried message array or whatever

            //display filled stack
            await Clients.Caller.JoinGroup();
        }
        public async Task Hello()
        {
         
                await Clients.All.hello();
            }







            //// max200 msg
            //if (msgpgroup.Count >= 199)
            //{
            //    msgpgroup.Dequeue();
            //    msgpgroup.Enqueue(sJSON);
            //}
            //else
            //{
            //    msgpgroup.Enqueue(sJSON);
            //}

            //then somehow yeet msgpgroup into db









        }
        //public DateTime GetServerDateTime()
        //{
        //    return DateTime.Now;
        //}



        //SELECT m.GroupID FROM MessageGroups m INNER JOIN Users u ON m.groupID = u.GroupID WHERE u.UserID = "John"
        // and then yea show the groups and shit


        public async Task Mute()
        {

            //retrieve group database
            //a cell with a json array showing all muted people.
            // var query1 = context.Messages.Where(s => s.ID == 123).FirstOrDefault<Message>();
            //get the results, put into an array of sort.
            //loop through if muted, <unmute option>
            //if not <mute option>
            //<mute> append a name to the array
            //<unmute> remove the name from the array

            await Clients.Caller.Mute();
        }
        public async Task ShowGroups()
        {

            using (var context = new ApplicationDbContext())
            {
                using (var ctxtransaction = context.Database.BeginTransaction())
                {


                    //  var query = context.Users.SqlQuery("SELECT Group_Id FROM AspNetUsers").To
                    //  var query = context.Database.ExecuteSqlCommand("SELECT User_Id FROM AspNetUsers").ToString();
                    //  Debug.Write(query);
                    //  var query3 = context.Events.SqlQuery("SELECT User_Id FROM AspNetUsers");
                    //  Debug.Write(query3);

                    //var query2 = context.Database.ExecuteSqlCommand("SELECT User_Id FROM Event").ToString();
                    //  Debug.Write(query2);

                    //  var query4 = context.Events.Where(s => s.AllDay == true);

                    //  Debug.Write(query4);

                    var query5 = context.Events.SqlQuery("SELECT * FROM Event").ToList();
                    Debug.Write(query5);
                    foreach (object o in query5)
                    {
                        Debug.Write(o.ToString());
                    }

                }
            }

            await Clients.Caller.ShowGroups();
        }
        public async Task ChangeRoom(string room)
        {


            await Clients.Caller.changeRoom();
        }

        public async Task ListGroup(string uid)
        {

            using (var ctx = new ApplicationDbContext())
            {
                LionFishWeb.Models.Group[] al = new LionFishWeb.Models.Group[10];
                var arr = ctx.Groups.SqlQuery("SELECT ID FROM Group WHERE User_Id ='" + uid + "'").ToArray();


                await Clients.Caller.ListGroup(arr);

            }


        }

        public async Task feedbackSend(string rating, string message)
        {
            //get current session username
            string encodedR = System.Security.SecurityElement.Escape(rating);
            string encodedM = System.Security.SecurityElement.Escape(message);
            Feedback tmp = new Feedback(encodedR, encodedM);
            using (var ctx = new ApplicationDbContext())
            {
                using (var ctxtransaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        //somehow get the userID here or smth
                        bool x = HttpContext.Current.User.Identity.IsAuthenticated;
                        var m = HttpContext.Current.User.Identity.GetUserId();
                        ctx.Database.ExecuteSqlCommand("INSERT INTO Feedback(rate,feedbackMessage,Users_Id) VALUES ("+ encodedR + ",'" + encodedM + "','" + m + "')");
                        ctxtransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        Debug.Write(e);
                        ctxtransaction.Rollback();
                    }
                }

            }
       
            

            await Clients.Caller.feedbackSend();
        }

    }




}

