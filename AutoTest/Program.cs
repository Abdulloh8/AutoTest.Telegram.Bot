
using AutoTest.Models.Users;
using AutoTest.Models.Service;

using Newtonsoft.Json;
using System;
using JFA.Telegram.Console;
using Telegram.Bot.Types;
using File = System.IO.File;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using User = AutoTest.Models.Users.User;
using System.Runtime.Intrinsics.X86;
using Telegram.Bot.Types.InputFiles;

var json = File.ReadAllText("uzlotin.json");
var questions = JsonConvert.DeserializeObject<List<QuestionModel>>(json);
var TiskentQuestionCount = 10;
var questionsCount = questions.Count() / TiskentQuestionCount;

var users = new List<User>();

Read();

var botManager =new TelegramBotManager();
var bot = botManager.Create("6063803861:AAHzYUF8hpJ3TF3T583PNrxaIoIblZEtqiA");
botManager.Start(OnUpdate);

void OnUpdate(Update update)
{

    var user = CheckUser(update);

    string message = "";

    if (update.Type == UpdateType.CallbackQuery)
    {
        message = update.CallbackQuery.Data;

    }
    else if (update.Type == UpdateType.Message)
    {
        message = update.Message.Text;
    }

    switch (user.Step)
    {
        case EUserStep.Default: StartTest(user); break;
        case EUserStep.InMenu: Menu(user, message); break;
        case EUserStep.start: start(update, user, message); break;
        case EUserStep.ticketPure: ticketPure(user, message, update); break;
    }           
            
    return;
}

void start(Update update,User user, string Message)
{
    if (update.Type == UpdateType.CallbackQuery)
    {
        bool f = update.CallbackQuery.Data.ToLower() == "true";
        bot.SendTextMessageAsync(user.ChatId, $"{update.CallbackQuery.Data}");
        
        if (f)
            user.CorrectCount++;
    }


    if (user.questionCount != TiskentQuestionCount)
    {
        if (update.Type == UpdateType.CallbackQuery)
        {
            Show(user);
        }
        else
        {
            bot.SendTextMessageAsync(user.ChatId, $"knopkani bos");
        }
    }
    else
    {
        user.Step = EUserStep.InMenu;

        if (user.CorrectCount == 10)
        {
            user.result[user.TicketNumber + 1] = $"{user.TicketNumber + 1} ✅";
            user.Userresult[user.TicketNumber + 1].text = $"{user.TicketNumber + 1} ✅";
            user.Userresult[user.TicketNumber + 1].full = true;
            user.Userresult[user.TicketNumber + 1].chala = false;
            Save();
        }
        else
        {
            user.result[user.TicketNumber + 1] = $"{user.TicketNumber + 1} /{user.CorrectCount}";
            user.Userresult[user.TicketNumber + 1].text = $"{user.TicketNumber + 1} /{user.CorrectCount}";
            user.Userresult[user.TicketNumber + 1].chala = true;
            user.Userresult[user.TicketNumber + 1].full = false;

            Save();
        }
        bot.SendTextMessageAsync(user.ChatId, $"{user.CorrectCount} ta tug'ri javob");
    }
}

User CheckUser(Update update)
{
    long chatId;

    if (update.Type == UpdateType.Message)
    {
        chatId = update.Message!.From!.Id;
    }
    else
    {
        chatId = update.CallbackQuery!.From!.Id;
    }
    
    User? user = users.FirstOrDefault(u => u.ChatId == chatId);

    if (user == null)
    {
        user = new User();
        user.Userresult = new List<UserRez>();
        user.ChatId = chatId;
        user.Name = update.Message.ForwardSenderName;
        List<string> name = new List<string>();
        name.Add($"{user.Name}");
        user.result = name;
        for (int i = 1; i <= 71; i++)
        {
            UserRez userRezz = new UserRez();

            userRezz.text = $"{i}";
            userRezz.full = false;
            userRezz.chala = false;

            user.Userresult.Add(userRezz);
            user.result.Add($"{i}");
        }
        user.Step = EUserStep.Default;
        user.ticketMenu = 1;
        users.Add(user);
    }

    return user;
}

void StartQuestion(User user)
{
    var random = new Random();
    var ticket = random.Next(0, questionsCount);
    user.TicketNumber = ticket;
    user.StartNumber = ticket * TiskentQuestionCount;
    user.ticketFinsh = ticket * TiskentQuestionCount + TiskentQuestionCount;
    user.questionCount = 0;
    user.CorrectCount = 0;
    bot.SendTextMessageAsync(user.ChatId, $"your Ticket {ticket + 1}");
    Show(user);
}

void StartTicketNumber(User user)
{

    var ticket = user.TicketNumber;
    user.StartNumber = ticket * TiskentQuestionCount;
    user.ticketFinsh = ticket * TiskentQuestionCount + TiskentQuestionCount;
    user.questionCount = 0;
    user.CorrectCount = 0;
    bot.SendTextMessageAsync(user.ChatId, $"your Ticket {ticket + 1}");
    Show(user);
}

void Show(User user)
{
    var question = questions[user.StartNumber + user.questionCount];
    var choiceButtons = new List<List<InlineKeyboardButton>>();
    var message = $"{question.Id} {question.Question}\n\n";
    char harf = 'A';
    for (int i = 0; i < question.choices.Count; i++)
    {
        message += $"{harf} {question.choices[i].Text}\n\n";
        var choiceButton = new List<InlineKeyboardButton>()
        {
            
            InlineKeyboardButton.WithCallbackData($"{harf}   {question.choices[i].Answer}",$"{question.choices[i].Answer}")
            
        };
        choiceButtons.Add(choiceButton);
        harf++;
    }
    user.questionCount++;

    
    if (question.Media.Exist)
    {
            var filebytes = File.ReadAllBytes($"Autotest/{question.Media.Name}.png");
            var ms = new MemoryStream(filebytes);

            bot.SendPhotoAsync(
            user.ChatId,
            photo: new InputOnlineFile(ms),
            caption: message,
            replyMarkup: new InlineKeyboardMarkup(choiceButtons)
            );
    }
    else
    {
        bot.SendTextMessageAsync(user.ChatId, message, replyMarkup: new InlineKeyboardMarkup(choiceButtons));
    }

}

void StartTest(User user)
{
    user.Step = EUserStep.InMenu;
    var keyboard = new ReplyKeyboardMarkup(new List<List<KeyboardButton>>()
    {
        new List<KeyboardButton>()
        {
            new KeyboardButton("Start")
        },
        new List<KeyboardButton>()
        {
            new KeyboardButton("ShowMenu")
        },
        new List<KeyboardButton>()
        {
            new KeyboardButton("ShowResultFull")
        },
        new List<KeyboardButton>()
        {
            new KeyboardButton("Full")
        }
    });

    keyboard.ResizeKeyboard = true;

    bot.SendTextMessageAsync(user.ChatId, "nma bu" ,replyMarkup: keyboard);
}

void Menu(User user, string Message)
{
    switch (Message)
    {
        case "Start": 
            {
              user.Step = EUserStep.start;
                StartQuestion(user);
            }
            break;
        case "ShowMenu":
            {
                ShowResult(user);
            }
            break;
        case "ShowResultFull":
            {
                ShowResultFull(user);
            }
            break;
        case "Full":
            {
                Full(user);
            }
            break;
        default:
            {
                bot.SendTextMessageAsync(user.ChatId, $"Mavjud bulmagan qiymat");
            }
            break;
    }
}

void ShowResult(User user)
{
    var row = new List<List<InlineKeyboardButton>>();
    var i = user.ticketMenu;

    var productNameButton = InlineKeyboardButton.WithCallbackData($"{user.result[i]}", $"{i - 1}");
    var productNameButton2 = InlineKeyboardButton.WithCallbackData($"{user.result[i + 1]}", $"{i}");
    var productNameButton3 = InlineKeyboardButton.WithCallbackData($"{user.result[i + 2]}", $"{i + 1}");
    var productNameButton4 = InlineKeyboardButton.WithCallbackData($"{user.result[i + 3]}", $"{i + 2}");
    var productNameButton5 = InlineKeyboardButton.WithCallbackData($"{user.result[i + 4]}", $"{i + 3}");

    row.Add(new List<InlineKeyboardButton>() { productNameButton, productNameButton2, productNameButton3, productNameButton4, productNameButton5 });

    var productNameButton10 = InlineKeyboardButton.WithCallbackData($"<3", $"<3");
    var productNameButton11 = InlineKeyboardButton.WithCallbackData($"<2", $"<2");
    var productNameButton12 = InlineKeyboardButton.WithCallbackData($"2>", $"2>");
    var productNameButton13 = InlineKeyboardButton.WithCallbackData($"3>", $"3>");

    if (i >= 66 || i == 61)
    {
        row.Add(new List<InlineKeyboardButton>() { productNameButton10, productNameButton11 });
    }
    else if (i <= 5 || i == 6)
    {
        row.Add(new List<InlineKeyboardButton>() { productNameButton12, productNameButton13 });
    }
    else if (i == 11)
    {
        row.Add(new List<InlineKeyboardButton>() { productNameButton11, productNameButton12, productNameButton13 });
    }
    else if (i == 56 )
    {
        row.Add(new List<InlineKeyboardButton>() { productNameButton10, productNameButton11, productNameButton12 });
    }
    else
    {
        row.Add(new List<InlineKeyboardButton>() { productNameButton10, productNameButton11, productNameButton12, productNameButton13 });
    }
    
    var productNameButton6 = InlineKeyboardButton.WithCallbackData($"<", $"<");
    var productNameButton7 = InlineKeyboardButton.WithCallbackData($">", $">");
    var productNameButton8 = InlineKeyboardButton.WithCallbackData($"Menu", $"Menu");

    if (i >= 66)
    {
        row.Add(new List<InlineKeyboardButton>() { productNameButton6, productNameButton8 });
    }
    else if (i <= 1)
    {
        row.Add(new List<InlineKeyboardButton>() { productNameButton7, productNameButton8 });
    }
    else
    {
        row.Add(new List<InlineKeyboardButton>() { productNameButton6, productNameButton7, productNameButton8 });
    }
    
    user.Step = EUserStep.ticketPure;

    bot.SendTextMessageAsync(user.ChatId,$"olma pish og'zimga tush", replyMarkup: new InlineKeyboardMarkup(row));
}

void ticketPure(User user,string Message, Update update)
{
    if (update.Type == UpdateType.CallbackQuery)
    {
        if (Message == "<")
        {
            user.ticketMenu -= 5;
            ShowResult(user);
        }
        else if (Message == ">")
        {
            user.ticketMenu += 5;
            ShowResult(user);
        }
        else if (Message == "Menu")
        {
            user.Step = EUserStep.InMenu;
            bot.SendTextMessageAsync(user.ChatId, $"Menu");
            StartTest(user);
        }
        else if (Message == "<3")
        {
            user.ticketMenu -= 15;
            ShowResult(user);
        }
        else if (Message == "<2")
        {
            user.ticketMenu -= 10;
            ShowResult(user);
        }
        else if (Message == "2>")
        {
            user.ticketMenu += 10;
            ShowResult(user);
        }
        else if (Message == "3>")
        {
            user.ticketMenu += 15;
            ShowResult(user);
        }
        else
        {
            int number = (int)Convert.ToUInt32(Message);
            user.TicketNumber = number;
            user.Step = EUserStep.start;
            StartTicketNumber(user);
        }
    }
    else
    {
        bot.SendTextMessageAsync(user.ChatId, $"knopkani bos");
    }
}

void ShowResultFull(User user)
{

    var row = new List<List<InlineKeyboardButton>>();

    

    for (int i = 1; i <= 70; i++)
    {
        if (user.Userresult[i].chala)
        {
            var productNameButton3 = InlineKeyboardButton.WithCallbackData($"{user.result[i]}", $"{i-1}");

            row.Add(new List<InlineKeyboardButton>() { productNameButton3 });
        }
    }
    var productNameButton8 = InlineKeyboardButton.WithCallbackData($"Menu", $"Menu");
    row.Add(new List<InlineKeyboardButton>() { productNameButton8 });
    user.Step = EUserStep.ticketPure;
    bot.SendTextMessageAsync(user.ChatId, $"Hamma ishlanganlar", replyMarkup: new InlineKeyboardMarkup(row));


}

void Full(User user)
{
    var row = new List<List<InlineKeyboardButton>>();



    for (int i = 1; i <= 70; i++)
    {
        if (user.Userresult[i].full)
        {
            var productNameButton3 = InlineKeyboardButton.WithCallbackData($"{user.result[i]}");

            row.Add(new List<InlineKeyboardButton>() { productNameButton3 });
        }
    }
    user.Step = EUserStep.InMenu;
    bot.SendTextMessageAsync(user.ChatId, $"Yakunlanganlar", replyMarkup: new InlineKeyboardMarkup(row));
}

void Save()
{
    string text = JsonConvert.SerializeObject(users);

    File.WriteAllText("users.json", text);
}

void Read()
{
    string text = File.ReadAllText("users.json");

    users = JsonConvert.DeserializeObject<List<User>>(text)!;

}





