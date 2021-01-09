# Vk2TgBot

Telegram бот, который отправляет посты из групп ВКонтакте в Tg каналы.

## Возможности бота

Вы можете настроить рассылку из групп ВК в определенные Tg каналы, а также посмотреть список уже добавленных ВК групп.

## Установка на Raspberry Pi 2+

1. Установка .NET 5 SDK

[Полная инструкция от Microsoft](https://dotnet.microsoft.com/download/dotnet/5.0 "Microsoft Download .NET 5.0")

Для того, чтобы dotnet команды были всегда доступны, необходимо добавить путь к dotnet в конец shell profile:
```bash
sudo nano ~/.profile

export DOTNET_ROOT=$HOME/dotnet
export PATH=$PATH:$HOME/dotnet
```
И затем перезагрузить Raspberry Pi.

2. Установка бота и запуск бота

Скачиваем проект и разархивируем проект в любое удобное место.
Затем необходимо дописать в Program.cs токен вашего бота, токен VK API и добавить имя пользователя Tg, для которого бот будет доступен.
Затем можем билдить проект:

```bash
cd <source root>/TelegramBot
dotnet publish -r linux-arm -c Release /p:PublishSingleFile=true --self-contained false --output ./published
./published/TelegramBot
```

## Управление ботом

Команды, которые понимает бот:
1. "/start" - Кратко опишет возможности бота.
2. "/list" - Получить список добавленных ВК групп и Telegram каналов.
3. "/listpc" - То же самое, что и "/list", но красиво оформлено в виде таблички. (На мобильных устройствах табличка съезжает).
4. "/add PublicName TgChannelId" - Привязывает к Tg каналу группу ВК для рассылки. 
5. "/remove PublicName TgChannelId" - Удаляет группу ВК из рассылки для Tg канала.

## Дополнительно

Библиотека System.Data.SQLite изначально не поддерживает ARM, поэтому необходимо вручную сбилдить ее на устройстве - [полная инструкция](http://blog.wezeku.com/2016/10/09/using-system-data-sqlite-under-linux-and-mono/). Я сделал это уже заранее и при компилировании проекта под Linux-arm, необходимые файлы динамеские библиотеки добавятся к исполняемому файлу. 
