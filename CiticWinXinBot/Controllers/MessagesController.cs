using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using CiticWinXinBot.Models;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace CiticWinXinBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {

        public Dictionary<string, string> dicEntity = new Dictionary<string, string>();
        public static double scoreStandard = 0.3;
        public Dictionary<string, string> ContextData = new Dictionary<string, string>();
        public bool isNeedDel = true;
        public int contextType = 0;
        public bool gotoEnd = false;
        public MessagesController()
        {
            dicEntity.Add("建投", "中信建投");
            dicEntity.Add("证券", "中信证券");
            dicEntity.Add("集团", "中信集团");
            dicEntity.Add("重工", "中信重工");
            dicEntity.Add("银行", "中信银行");
        }
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                //监视运行时间代码段
                //    System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
                //    stopwatch.Start();
                //    stopwatch.Stop(); //  停止监视
                //TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
                //double hours = timespan.TotalHours; // 总小时
                //double minutes = timespan.TotalMinutes;  // 总分钟
                //double seconds = timespan.TotalSeconds;  //  总秒数
                //double milliseconds = timespan.TotalMilliseconds;  //  
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;

                // return our reply to the user
                Activity reply;

                //获取用户信息，为设置上下文做准备
                StateClient sc = activity.GetStateClient();
                BotData botData = sc.BotState.GetUserData(activity.ChannelId, activity.From.Id);
               
                
                //ContextData = new Dictionary<string, string>();
                string contextMessage = "";
                contextMessage = botData.GetProperty<string>("指令");
              
                if (contextMessage != "")
                {
                  
                    if (contextMessage == "查询报销")
                    {
                       contextType = 1;//1表示查询报销
                    }
                    if (contextMessage == "查询天气")
                    {
                        string replyString = await GetWeather(activity.Text);
                        if (replyString != "")
                        {
                            reply = activity.CreateReply(replyString);
                        }
                        else
                        {
                            replyString = string.Format("呃。。。花卷目测\"{0}\"这个应该不是一个城市的名字。。不然我咋不知道呢。。。请询问类似《查询XX的天气》的话语", activity.Text);
                            reply = activity.CreateReply(replyString);
                        }
                        await connector.Conversations.ReplyToActivityAsync(reply);
                        gotoEnd = true;
                    }
                }
                else
                {
                    isNeedDel = false;
                }

                if (activity.Text.Contains("世界上最美丽的人是谁"))
                {
                    contextType = 0;
                    reply = activity.CreateReply($"我觉得您就是世界上最美丽的人");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else if (activity.Text.Contains("讲个笑话"))
                {
                    contextType = 0;
                    var Tulingresult = await LUISLibrary.LUISHelper.MakeRequestTuling(activity.Text, activity.Conversation.Id);
                    reply = activity.CreateReply(Tulingresult.text);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else if (activity.Text.Contains("帮助"))
                {
                    contextType = 0;
                    string resultmessage = "您好，我是中信集团企业小助手花卷，我已经学会了如下能力供您体验：\r\n1.查询新闻和动态\r\n例如：询问查询XXX的新闻/看看有什么消息/XX头条等\r\n2.查询报销\r\n例如：我要查询报销/报销等\r\n查询报销后，可查询报销被拒绝的原因\r\n例如：查询第二项被拒绝原因等\r\n3.查询活动消息\r\n例如：查询中华魂活动等等\r\n4.查询天气（需要提供城市名称）\r\n例如：查询XX天气/XX天气情况\r\n5.发起聊天，组成聊天进行会话\r\n例如：联系XXX、YYY/与XX和YY联系/联系XXX等\r\n6.一些问候等调侃会话\r\n目前还支持不多，请随意尝试\r\n后续花卷将会学习更多功能和会话内容，请对我充满期待呦~";
                    reply = activity.CreateReply(resultmessage);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else if (activity.Text.Contains("世界上最帅的人是谁"))
                {
                    contextType = 0;
                    reply = activity.CreateReply($"我觉得您就是世界上最帅的人");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else if (activity.Text.Contains("同志们辛苦啦") || activity.Text.Contains("同志辛苦啦"))
                {
                    contextType = 0;
                    //todo:当两个intents 比分相近时应该询问用户来确认
                    reply = activity.CreateReply($"为人民服务");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else if (gotoEnd)
                {

                }
                else
                {
                    var Luisresult = await LUISLibrary.LUISHelper.MakeRequest(activity.Text, 1);
                    if (Luisresult.LUISIntent == null)
                    {
                        contextType = 0;
                        reply = activity.CreateReply($"对不起>.<，我还在努力学习，没有能够理解您的意思，能不能换一种询问方法，请回复帮助来查看我已经学会的能力。谢谢");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                    else
                    {
                        if (Luisresult.LUISIntent.score > scoreStandard)
                        {
                            if (Luisresult.LUISIntent.intent.ToString() == "问候")
                            {
                                contextType = 0;
                                if (Luisresult.LUISEntitys == null)
                                {
                                    reply = activity.CreateReply("您好，我还不知道怎样回复您的问候，请不要生我的气，我会继续努力学习的。我的能力可通过输入帮助来查看。谢谢");
                                }
                                else
                                {
                                    //TODO:
                                    string replyString = "";
                                    string resultMessage = "";
                                    foreach (LUISLibrary.Entity e in Luisresult.LUISEntitys)
                                    {
                                        if (e != null && e.type == "问候词")
                                        {
                                            resultMessage = e.entity;
                                            break;
                                        }

                                    }
                                    replyString = resultMessage;
                                    replyString += await GetWeather();
                                    replyString += "请问我有什么可以帮助您的吗？>.<~";
                                    reply = activity.CreateReply(replyString);
                                }
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                            else if (Luisresult.LUISIntent.intent.ToString() == "查询最新头条")
                            {
                                contextType = 0;
                                reply = activity.CreateReply($"头条");
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }

                            else if (Luisresult.LUISIntent.intent.ToString() == "查询动态")
                            {
                                contextType = 0;
                                if (Luisresult.LUISEntitys == null || Luisresult.LUISEntitys[0].entity.Contains("集团"))
                                {
                                    reply = activity.CreateReply("动态,集团");
                                }
                                else
                                {
                                    //TODO:
                                    reply = activity.CreateReply("动态," + formatEntityName(Luisresult.LUISEntitys[0].entity));
                                }
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }

                            else if (Luisresult.LUISIntent.intent.ToString() == "求称赞")
                            {
                                contextType = 0;
                                if (Luisresult.LUISEntitys == null)
                                {
                                    reply = activity.CreateReply("您好，我还不知道怎样回复您,请不要生气，我会继续努力学习哒~~");
                                }
                                else
                                {
                                    string resultMessage = "";
                                    foreach (LUISLibrary.Entity e in Luisresult.LUISEntitys)
                                    {
                                        if (e != null && e.type == "称赞")
                                        {
                                            resultMessage = getCompliment(e.entity);
                                            break;
                                        }

                                    }
                                    if (resultMessage == "")
                                    {
                                        reply = activity.CreateReply("您好，我还不知道怎样回复您，请给我多一点时间，我会继续努力学习的。");
                                    }
                                    else
                                        reply = activity.CreateReply(resultMessage);
                                }
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                            else if (Luisresult.LUISIntent.intent.ToString() == "查询活动")
                            {
                                contextType = 0;
                                if (Luisresult.LUISEntitys == null)
                                {
                                    reply = activity.CreateReply("活动");
                                }
                                else
                                {
                                    string resultMessage = "";
                                    foreach (LUISLibrary.Entity e in Luisresult.LUISEntitys)
                                    {
                                        if (e != null && e.type == "活动对象")
                                        {
                                            resultMessage = resultMessage + e.entity;
                                            break;
                                        }

                                    }
                                    if (resultMessage == "")
                                    {
                                        reply = activity.CreateReply("活动");
                                    }
                                    else
                                        reply = activity.CreateReply("活动," + resultMessage);
                                }
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                            else if (Luisresult.LUISIntent.intent.ToString() == "查询报销")
                            {
                                reply = activity.CreateReply("报销");

                                //SaveBotData(activity, "查询报销", activity.Text);
                                //if (ContextData.ContainsKey("指令"))
                                //{
                                //    ContextData["指令"] = "查询报销";
                                //}
                                //else
                                //{
                                //    ContextData.Add("指令", "查询报销");
                                //}
                                //if (ContextData.ContainsKey("消息"))
                                //{
                                //    ContextData["消息"] = message;
                                //}
                                //else
                                //{
                                //    ContextData.Add("消息", message);
                                //}
                                isNeedDel = false;
                                botData.SetProperty<string>("指令", "查询报销");
                                sc.BotState.SetUserData(activity.ChannelId, activity.From.Id, botData);
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                            else if (Luisresult.LUISIntent.intent.ToString() == "询问拒绝原因")
                            {
                                //todo:要判断一下输入是否是整数字，不能是负数或者小数
                                
                                string resultMessage = "拒绝原因";
                                isNeedDel = false;
                                if (Luisresult.LUISEntitys == null)
                                {
                                    reply = activity.CreateReply(resultMessage);
                                }
                                else
                                {
                                    foreach (LUISLibrary.Entity e in Luisresult.LUISEntitys)
                                    {
                                        if (e != null && e.type == "number")
                                        {
                                            long num = ParseCnToInt(e.entity);
                                            resultMessage = resultMessage + "," + num.ToString();
                                            break;
                                        }

                                    }
                                    reply = activity.CreateReply(resultMessage);
                                }

                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                            else if (Luisresult.LUISIntent.intent.ToString() == "数字选择内容")
                            {
                                //todo:要判断一下输入是否是数字
                                long num;
                                if (Luisresult.LUISEntitys == null)
                                {
                                    reply = activity.CreateReply("您好，我还不知道怎样回复您，请对我多一点耐心，我会继续努力学习的。我的能力可通过输入帮助来查看。谢谢");
                                }
                                else
                                {
                                    num = -1;
                                    foreach (LUISLibrary.Entity e in Luisresult.LUISEntitys)
                                    {
                                        if (e != null && e.type == "number")
                                        {
                                            num = ParseCnToInt(e.entity);
                                            break;
                                        }

                                    }
                                    if (num == -1)
                                        reply = activity.CreateReply("您好，我还不知道怎样回复您，请对我多一点耐心，我会继续努力学习的。我的能力可通过输入帮助来查看。谢谢");
                                    else
                                    {
                                        if (contextType == 1)
                                        {
                                            reply = activity.CreateReply("拒绝原因," + num.ToString());
                                            isNeedDel = false;
                                        }
                                        else
                                            reply = activity.CreateReply(num.ToString());
                                    }
                                }

                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                            else if (Luisresult.LUISIntent.intent.ToString() == "发起聊天")
                            {
                                contextType = 0;
                                string resultMessage = "联系";
                                if (Luisresult.LUISEntitys == null)
                                {
                                    reply = activity.CreateReply(resultMessage);
                                }
                                else
                                {
                                    foreach (LUISLibrary.Entity e in Luisresult.LUISEntitys)
                                    {
                                        if (e != null && e.type == "人名")
                                        {
                                            resultMessage = resultMessage + "," + e.entity;
                                        }

                                    }
                                    reply = activity.CreateReply(resultMessage);
                                }

                                await connector.Conversations.ReplyToActivityAsync(reply);

                            }
                            else if (Luisresult.LUISIntent.intent.ToString() == "查询天气")
                            {
                                contextType = 0;
                                string replyString = "";
                                //replyString = await GetWeather(entityInNone);
                                if (Luisresult.LUISEntitys == null)
                                {
                                    replyString = "亲你要查询哪个地方的天气信息呢，快把城市的名字发给我吧";
                                    isNeedDel = false;
                                    botData.SetProperty<string>("指令", "查询天气");
                                    sc.BotState.SetUserData(activity.ChannelId, activity.From.Id, botData);
                                    reply = activity.CreateReply(replyString);
                                }
                                else
                                {
                                    string location = "";
                                    foreach (LUISLibrary.Entity e in Luisresult.LUISEntitys)
                                    {
                                        if (e != null && e.type == "地点")
                                        {
                                            location = e.entity;
                                            replyString = await GetWeather(e.entity);
                                            break;
                                        }
                                    }
                                    if (replyString != "")
                                    {
                                        reply = activity.CreateReply(replyString);
                                    }
                                    else
                                    {
                                        replyString = string.Format("呃。。。花卷目测\"{0}\"这个应该不是一个城市的名字。。不然我咋不知道呢。。。请询问类似《查询XX的天气》的话语", location);
                                        reply = activity.CreateReply(replyString);
                                    }
                                }

                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                            else if (Luisresult.LUISIntent.intent.ToString() == "查询某人信息")
                            {
                                if (Luisresult.LUISEntitys == null)
                                {
                                    reply = activity.CreateReply("");
                                }
                                else
                                {
                                    string resultMessage = "";
                                    string defiList = "常振明,王炯,朱皋鸣,蔡华相,李庆萍,蒲坚,蔡希良,冯光,常老板,常总,朱总,炯总";
                                    foreach (LUISLibrary.Entity e in Luisresult.LUISEntitys)
                                    {
                                        if (e != null && e.type == "人名")
                                        {
                                            if (defiList.Contains(e.entity))
                                            {
                                                if (e.entity == "朱总")
                                                    resultMessage = resultMessage + ",朱皋鸣";
                                                else if (e.entity == "常总" || e.entity == "常老板")
                                                    resultMessage = resultMessage + ",常振明";
                                                else resultMessage = resultMessage + "," + e.entity;
                                            }
                                        }
                                    }
                                    if (resultMessage != "")
                                        reply = activity.CreateReply("动态" + resultMessage);
                                    else reply = activity.CreateReply("");
                                }
                                await connector.Conversations.ReplyToActivityAsync(reply);

                            }
                            else if (Luisresult.LUISIntent.intent.ToString() == "查询帮助")
                            {
                                contextType = 0;
                                string resultmessage = "您好，我是中信集团企业小助手花卷，我已经学会了如下能力供您体验：\r\n1.查询新闻和动态\r\n例如：询问查询XXX的新闻/看看有什么消息/XX头条等\r\n2.查询报销\r\n例如：我要查询报销/报销等\r\n查询报销后，可查询报销被拒绝的原因\r\n例如：查询第二项被拒绝原因等\r\n3.查询活动消息\r\n例如：查询中华魂活动等等\r\n4.查询天气（需要提供城市名称）\r\n例如：查询XX天气/XX天气情况\r\n5.发起聊天，组成聊天进行会话\r\n例如：联系XXX、YYY/与XX和YY联系/联系XXX等\r\n6.一些问候等调侃会话\r\n目前还支持不多，请随意尝试\r\n后续花卷将会学习更多功能和会话内容，请对我充满期待呦~";
                                reply = activity.CreateReply(resultmessage);
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                            else if (Luisresult.LUISIntent.intent.ToString() == "日常调侃")
                            {
                                contextType = 0;
                                string mss = "";
                                var Tulingresult = await LUISLibrary.LUISHelper.MakeRequestTuling(activity.Text, activity.Conversation.Id);
                                //100000文字类
                                if (Tulingresult.code == "100000")
                                {

                                    mss = Tulingresult.text;
                                }
                                else if (Tulingresult.code == "40001")
                                {   //参数key错误
                                    mss = "我不知道怎样回复了，40001";
                                }
                                else if (Tulingresult.code == "40002")
                                {   //请求内容info为空
                                    mss = "我不知道怎样回复了，40002";
                                }
                                else if (Tulingresult.code == "40004")
                                {   //当天请求次数已使用完
                                    mss = "我不知道怎样回复了，40004";
                                }
                                else if (Tulingresult.code == "40007")
                                {   //数据格式异常/请按规定的要求进行加密
                                    mss = "我不知道怎样回复了，40007";
                                }

                                if (mss == "")
                                {
                                    //TODO:现在写死消息，实际上要回复没获得有效回复
                                    mss = Tulingresult.text;
                                }
                                reply = activity.CreateReply(mss);
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                            else
                            {
                                contextType = 0;
                                //reply = activity.CreateReply($"我还不知道怎样理解您的问题，我会继续努力学习的，请不要责怪我。我的能力可通过输入帮助来查看。谢谢");
                                //await connector.Conversations.ReplyToActivityAsync(reply);
                                string mss = "";
                                var Tulingresult = await LUISLibrary.LUISHelper.MakeRequestTuling(activity.Text, activity.Conversation.Id);
                                //100000文字类
                                if (Tulingresult.code == "100000")
                                {
                                    mss = Tulingresult.text;
                                }
                                else if (Tulingresult.code == "40001")
                                {   //参数key错误
                                    mss = "我不知道怎样回复了，40001";
                                }
                                else if (Tulingresult.code == "40002")
                                {   //请求内容info为空
                                    mss = "我不知道怎样回复了，40002";
                                }
                                else if (Tulingresult.code == "40004")
                                {   //当天请求次数已使用完
                                    mss = "我不知道怎样回复了，40004";
                                }
                                else if (Tulingresult.code == "40007")
                                {   //数据格式异常/请按规定的要求进行加密
                                    mss = "我不知道怎样回复了，40007";
                                }

                                if (mss == "")
                                {
                                    //TODO:现在写死消息，实际上要回复没获得有效回复
                                    mss = Tulingresult.text;
                                }
                                reply = activity.CreateReply(mss);
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                        }
                        else
                        {
                            contextType = 0;
                            //reply = activity.CreateReply($"我还不知道怎样理解您的问题，我会继续努力学习的，请不要责怪我。我的能力可通过输入帮助来查看。谢谢");
                            //await connector.Conversations.ReplyToActivityAsync(reply);
                            string mss = "";
                            var Tulingresult = await LUISLibrary.LUISHelper.MakeRequestTuling(activity.Text, activity.Conversation.Id);
                            //100000文字类
                            if (Tulingresult.code == "100000")
                            {
                                mss = Tulingresult.text;
                            }
                            else if (Tulingresult.code == "40001")
                            {   //参数key错误
                                mss = "我不知道怎样回复了，40001";
                            }
                            else if (Tulingresult.code == "40002")
                            {   //请求内容info为空
                                mss = "我不知道怎样回复了，40002";
                            }
                            else if (Tulingresult.code == "40004")
                            {   //当天请求次数已使用完
                                mss = "我不知道怎样回复了，40004";
                            }
                            else if (Tulingresult.code == "40007")
                            {   //数据格式异常/请按规定的要求进行加密
                                mss = "我不知道怎样回复了，40007";
                            }

                            if (mss == "")
                            {
                                //TODO:现在写死消息，实际上要回复没获得有效回复
                                mss = Tulingresult.text;
                            }
                            reply = activity.CreateReply(mss);
                            await connector.Conversations.ReplyToActivityAsync(reply);
                        }
                    }

                }
                if (contextType == 0 && isNeedDel == true)
                {
                    DeleteBotData(activity);
                }
            }
            // await Conversation.SendAsync(activity, () => new MengmengDialog());
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }


        public  void SaveBotData(Activity item, string command, string message)
        {
            StateClient sc = item.GetStateClient();
            BotData bd = sc.BotState.GetUserData(item.ChannelId, item.From.Id);
            if (ContextData.ContainsKey("指令"))
            {
                ContextData["指令"] = command;
            }
            else
            {
                ContextData.Add("指令", command);
            }
            if (ContextData.ContainsKey("消息"))
            {
                ContextData["消息"] = message;
            }
            else
            {
                ContextData.Add("消息", message);
            }
            bd.SetProperty<Dictionary<string,string>>("指令", ContextData);

            sc.BotState.SetUserData(item.ChannelId, item.From.Id, bd);
        }

        public void GetBotData(Activity item)
        {
            StateClient sc = item.GetStateClient();
           
            BotData botData = sc.BotState.GetUserData(item.ChannelId, item.From.Id);
            ContextData = new Dictionary<string, string>();
           
            ContextData= botData.GetProperty<Dictionary<string, string>>("指令");
         }
        public async void DeleteBotData(Activity item)
        {
            StateClient sc = item.GetStateClient();
            await sc.BotState.DeleteStateForUserAsync(item.ChannelId, item.From.Id);
        }


        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        [LuisModel("c5a82f9a-02db-472c", "bc983f3ae6a44ca7b")]
        [Serializable]
        public class MengmengDialog : LuisDialog<object>
        {
            public MengmengDialog()
            {
            }
            public MengmengDialog(ILuisService service)
            : base(service)
            {
            }
            [LuisIntent("")]
            public async Task None(IDialogContext context, LuisResult result)
            {
                string message = $"花卷不知道你在说什么，面壁去。。。我现在只会查询天气。。T_T" + string.Join(", ", result.Intents.Select(i => i.Intent));
                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }
            public bool TryToFindLocation(LuisResult result, out String location)
            {
                location = "";
                EntityRecommendation title;
                if (result.TryFindEntity("地点", out title))
                {
                    location = title.Entity;
                }
                else
                {
                    location = "";
                }
                return !location.Equals("");
            }
            [LuisIntent("查询天气")]
            public async Task QueryWeather(IDialogContext context, LuisResult result)
            {
                string location = "";
                string replyString = "";
                if (TryToFindLocation(result, out location))
                {
                   // replyString = await GetWeather(location);
                    await context.PostAsync(replyString);
                    context.Wait(MessageReceived);
                }
                else
                {
                    await context.PostAsync("亲你要查询哪个地方的天气信息呢，快把城市的名字发给我吧");
                    //context.Wait(AfterEnterLocation);
                }
            }




        }


        public void ChangeCid(string m_page)
        {
            string pattern = "<img\\s*[\\S]*\\s+src=[\"\']?([^\"\'\\s]+)[\"\']?[\\s>][^(/>)]+";
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(m_page);
            //string pageTmp = m_page;
            //int count = 0;
            //int indexTmp = 0;
            foreach (Match nextmatch in matches)
            {
                //string cid = string.Format("<img src='cid:{0}'", m_cid[indexTmp]);
                //indexTmp++;
                //pageTmp = m_page.Substring(0, nextmatch.Index - count);
                //pageTmp += cid;
                //pageTmp += m_page.Substring(nextmatch.Index + nextmatch.Length - count);
                //count += nextmatch.Length - cid.Length;
                //m_page = pageTmp;
            }
        }

        public string formatEntityName(string entity)
        {
            if (entity == null || entity == "")
                return "";
            string result = entity;
            if (dicEntity.ContainsKey(entity))
                result = dicEntity[entity];
            return result;
        }

        /// <summary>  
        /// 转换数字  
        /// </summary>
        protected static long CharToNumber(char c)
        {
            switch (c)
            {
                case '一': return 1;
                case '二': return 2;
                case '三': return 3;
                case '四': return 4;
                case '五': return 5;
                case '六': return 6;
                case '七': return 7;
                case '八': return 8;
                case '九': return 9;
                case '零': return 0;
                default: return -1;
            }
        }

        /// <summary>  
        /// 转换单位  
        /// </summary>  
        protected static long CharToUnit(char c)
        {
            switch (c)
            {
                case '十': return 10;
                case '百': return 100;
                case '千': return 1000;
                case '万': return 10000;
                case '亿': return 100000000;
                default: return 1;
            }
        }

        public static long ParseCnToInt(string cnum)
        {
            long firstUnit = 1;//一级单位                  
            long secondUnit = 1;//二级单位   
            long tmpUnit = 1;//临时单位变量  
            long result = 0;//结果
            if (long.TryParse(cnum,out result))
            {
                return result;
            }
            cnum = Regex.Replace(cnum, "\\s+", "");
              
            for (int i = cnum.Length - 1; i > -1; --i)//从低到高位依次处理  
            {
                tmpUnit = CharToUnit(cnum[i]);//取出此位对应的单位  
                if (tmpUnit > firstUnit)//判断此位是数字还是单位  
                {
                    firstUnit = tmpUnit;//是的话就赋值,以备下次循环使用  
                    secondUnit = 1;
                    if (i == 0)//处理如果是"十","十一"这样的开头的  
                    {
                        result += firstUnit * secondUnit;
                    }
                    continue;//结束本次循环  
                }
                else if (tmpUnit > secondUnit)
                {
                    secondUnit = tmpUnit;
                    continue;
                }
                result += firstUnit * secondUnit * CharToNumber(cnum[i]);//如果是数字,则和单位想乘然后存到结果里  
            }
            return result;
        }


        private async Task<string> GetWeather(string cityname)
        {
            WeatherData weatherdata = await GetWeatherAsync(cityname);
            if (weatherdata == null || weatherdata.HeWeatherdataservice30 == null)
            {
                return string.Format("呃。。。花卷不知道\"{0}\"这个城市的天气信息", cityname);
            }
            else
            {
                HeweatherDataService30[] weatherServices = weatherdata.HeWeatherdataservice30;
                if (weatherServices.Length <= 0) return string.Format("呃。。。花卷不知道\"{0}\"这个城市的天气信息", cityname);
                Basic cityinfo = weatherServices[0].basic;
                if (cityinfo == null) return string.Format("呃。。。花卷目测\"{0}\"这个应该不是一个城市的名字。。不然我咋不知道呢。。。", cityname);
                String cityinfoString = "城市信息：" + cityinfo.city + "\r\n"
                    + "更新时间:" + cityinfo.update.loc + "\r\n"
                    + "经纬度:" + cityinfo.lat + "," + cityinfo.lon + "\r\n";
                Aqi cityAirInfo = weatherServices[0].aqi;
                String airInfoString = "空气质量指数：" + cityAirInfo.city.aqi + "\r\n"
                    + "PM2.5 1小时平均值：" + cityAirInfo.city.pm25 + "(ug/m³)\r\n"
                    + "PM10 1小时平均值：" + cityAirInfo.city.pm10 + "(ug/m³)\r\n"
                    + "二氧化硫1小时平均值：" + cityAirInfo.city.so2 + "(ug/m³)\r\n"
                    + "二氧化氮1小时平均值：" + cityAirInfo.city.no2 + "(ug/m³)\r\n"
                    + "一氧化碳1小时平均值：" + cityAirInfo.city.co + "(ug/m³)\r\n";

                Suggestion citySuggestion = weatherServices[0].suggestion;
                String suggestionString = "生活指数：" + "\r\n"
                    + "穿衣指数：" + citySuggestion.drsg.txt + "\r\n"
                    + "紫外线指数：" + citySuggestion.uv.txt + "\r\n"
                    + "舒适度指数：" + citySuggestion.comf.txt + "\r\n"
                    + "旅游指数：" + citySuggestion.trav.txt + "\r\n"
                    + "感冒指数：" + citySuggestion.flu.txt + "\r\n";

                Daily_Forecast[] cityDailyForecast = weatherServices[0].daily_forecast;
                Now cityNowStatus = weatherServices[0].now;
                String nowStatusString = "天气实况：" + "\r\n"
                    + "当前温度(摄氏度)：" + cityNowStatus.tmp + "\r\n"
                    + "体感温度：" + cityNowStatus.fl + "\r\n"
                    + "风速：" + cityNowStatus.wind.spd + "(Kmph)\r\n"
                    + "湿度：" + cityNowStatus.hum + "(%)\r\n"
                    + "能见度：" + cityNowStatus.vis + "(km)\r\n";

                return string.Format("花卷天气播报：\r\n{0}", cityinfoString + nowStatusString + airInfoString + suggestionString);
            }
        }

        private async Task<string> GetWeather()
        {
            string cityname = "北京";
            WeatherData weatherdata = await GetWeatherAsync(cityname);
            if (weatherdata == null || weatherdata.HeWeatherdataservice30 == null)
            {
                return "";
            }
            else
            {
                HeweatherDataService30[] weatherServices = weatherdata.HeWeatherdataservice30;
                if (weatherServices.Length <= 0) return "";
                Basic cityinfo = weatherServices[0].basic;
                if (cityinfo == null) return "";
                Daily_Forecast[] cityDailyForecast = weatherServices[0].daily_forecast;
                string daulyForecastString = "\r\n今天天气" + cityDailyForecast[0].cond.txt_n+ "，\r\n";
                Now cityNowStatus = weatherServices[0].now;
                String nowStatusString = "体感温度" + cityNowStatus.fl + "度，\r\n";
                string nowWindString = cityDailyForecast[0].wind.dir + cityDailyForecast[0].wind.sc + "级，\r\n";
                Aqi cityAirInfo = weatherServices[0].aqi;
                String airInfoString ="PM2.5：" + cityAirInfo.city.pm25 + "，\r\n";
                Random rd = new Random();
                int i = rd.Next(1, 5);
                Suggestion citySuggestion = weatherServices[0].suggestion;

                String suggestionString = "";
                if (i == 1)
                    suggestionString = citySuggestion.drsg.txt;
                else if (i == 2)
                    suggestionString = citySuggestion.uv.txt;
                else if (i == 3)
                    suggestionString = citySuggestion.comf.txt;
                else if (i == 4)
                    suggestionString = citySuggestion.trav.txt;
                else
                    suggestionString = citySuggestion.flu.txt;
                return string.Format("{0}\r\n", daulyForecastString + nowStatusString + nowWindString + airInfoString + suggestionString);
            }
        }

        public static async Task<WeatherData> GetWeatherAsync(string city)
        {
            try
            {
                string ServiceURL = $"https://api.heweather.com/x3/weather?city={city}&key=f0816afd4a3e4409aa4831cf16b4d00b";
                string ResultString;
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    ResultString = await client.DownloadStringTaskAsync(ServiceURL).ConfigureAwait(false);
                }
                WeatherData weatherData = (WeatherData)JsonConvert.DeserializeObject(ResultString, typeof(WeatherData));
                return weatherData;
            }
            catch (WebException ex)
            {
                //handle your exception here  
                //throw ex;
                return null;
            }
        }

        public string getCompliment(string word)
        {
            string resultMessage = "";
            Random rd = new Random();
            int i = rd.Next(1, 3);
            switch (i)
            {
                case 1:
                    resultMessage = "千言万语，也难以修饰您是{0}的人";
                    break;
                case 2:
                    ; resultMessage = "在我的心中，没有人能够比您{0}";
                    break;
                case 3:
                    ; resultMessage = "天地为证，我真心实意的认为您是{0}的人";
                    break;
                default:
                    ;
                    break;

            }
            resultMessage = string.Format(resultMessage, word);
            return resultMessage;
        }

        public class ArithmeticExpressions
        {
            /// <summary>  
                    /// 获取中缀表达式  
                    /// </summary>  
                    /// <param name="exp">计算表达式（数字与运算符用空格分隔）</param>  
                    /// <returns></returns>  
            public static List<string> getColuExpression(string exp)
            {
                System.Text.ASCIIEncoding asc = new System.Text.ASCIIEncoding();
                Stack st = new Stack();
                string[] temp = exp.Split(' ');
                List<string> value = new List<string>();

                for (int i = 0; i < temp.Length; i++)
                {
                    int num = (int)asc.GetBytes(temp[i])[0];
                    if (num < 48 && num > 39)
                    {
                        if (st.Count > 0)
                        {
                            string operatorStr = st.Peek().ToString();
                            if (temp[i] == "*" || temp[i] == "/")
                            {
                                if (temp[i + 1] == "(")
                                {
                                    st.Push(temp[i]);
                                    continue;
                                }
                                else
                                {
                                    if (operatorStr == "(")
                                    {
                                        st.Push(temp[i]);
                                        continue;
                                    }
                                    else if (operatorStr == "*" || operatorStr == "/")
                                    {
                                        value.Add(st.Pop().ToString());
                                        st.Push(temp[i]);
                                        continue;
                                    }
                                    else
                                    {
                                        st.Push(temp[i]);
                                        continue;
                                    }
                                }
                            }
                            else if (temp[i] == "+" || temp[i] == "-")
                            {
                                if (operatorStr == "(")
                                {
                                    st.Push(temp[i]);
                                    continue;
                                }
                                else
                                {
                                    value.Add(st.Pop().ToString());
                                    if (st.Count > 0 && st.Peek().ToString() != "(")
                                    {
                                        value.Add(st.Pop().ToString());
                                    }
                                    st.Push(temp[i]);
                                    continue;
                                }
                            }
                            else if (temp[i] == "(")
                            {
                                st.Push(temp[i]);
                                continue;
                            }
                            else
                            {
                                if (i + 1 == temp.Length)
                                {
                                    value.Add(st.Pop().ToString());
                                    st.Pop();
                                    while (st.Count > 0)
                                        value.Add(st.Pop().ToString());
                                    break;
                                }
                                else
                                {
                                    value.Add(st.Pop().ToString());
                                    st.Pop();
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            st.Push(temp[i]);
                            continue;
                        }
                    }
                    else if (i + 1 == temp.Length)
                    {
                        value.Add(temp[i]);
                        value.Add(st.Pop().ToString());
                        while (st.Count > 0)
                            value.Add(st.Pop().ToString());
                        break;
                    }
                    else
                    {
                        value.Add(temp[i]);
                        continue;
                    }
                }
                return value;
            }

            /// <summary>  
                    /// 获取计算表达式的值  
                    /// </summary>  
                    /// <param name="expression">中缀表达式数组</param>  
                    /// <returns></returns>  
            public static double CalculateResult(string expre)
            {
                List<string> temp = getColuExpression(expre);
                try
                {
                    while (temp.Count > 1)
                    {
                        for (int i = 0; i < temp.Count; i++)
                        {
                            double resultTemp = 0;
                            if (temp[i] == "+")
                                resultTemp = Convert.ToDouble(temp[i - 2]) + Convert.ToDouble(temp[i - 1]);
                            else if (temp[i] == "-")
                                resultTemp = Convert.ToDouble(temp[i - 2]) - Convert.ToDouble(temp[i - 1]);
                            else if (temp[i] == "*")
                                resultTemp = Convert.ToDouble(temp[i - 2]) * Convert.ToDouble(temp[i - 1]);
                            else if (temp[i] == "/")
                                resultTemp = Convert.ToDouble(temp[i - 2]) / Convert.ToDouble(temp[i - 1]);
                            else
                                continue;
                            temp[i - 2] = resultTemp.ToString();
                            temp.RemoveAt(i);
                            temp.RemoveAt(i - 1);
                            break;
                        }
                    }
                }
                catch (Exception ex)//计算表达式的值错误，导致无法运算  
                {
                    temp.Clear();
                    temp.Add("0");
                }
                return Convert.ToDouble(temp[0]);
            }

           
        }
    }
}