# Проект. Возникновение идеи

Во время объявленной в России самоизоляции я наконец-таки стал обладателем аккаунта на **Spotify**, благодаря чему мои рекомендации в этом сервисе плавно заполнил западный рэп. Одним из вечеров я задался вопросом об истории становления такого канадского рэпера как **Дрейк**, так как в последние дни о нем говорило много западных СМИ по причине релиза его нового альбома.

Из полученной мной информации в сети я узнал, что Дрейк владеет собственным роскошным особняком, на территории которого как раз на днях (летом 2020-го) были закончены работы по созданию баскетбольного зала.

Фото и видео зала в сети было очень мало, но и из них я понял, что иметь такой баскетбольный зал (да и особняк, в принципе) - это очень круто, как по мне.

![1](https://cdn.vox-cdn.com/thumbor/HGCkAbVTwvXd3hNYxQfzXwZeeB8=/0x0:1600x1163/920x0/filters:focal(0x0:1600x1163):format(webp):no_upscale()/cdn.vox-cdn.com/uploads/chorus_asset/file/19884078/drake_court.jpg "1")

И именно в этот момент я подумал: а что, если создать 3D-копию, прикрутить к ней игровые механики, а на фоне с эффектом реверберации в зале крутить мои треки?
Так и появилась идея моего проекта, которая в дальнейшем обрастала новыми фишками и задумками.

# Что из себя представляет

Как я уже говорил ранее, в качестве основы служит баскетбольная площадка, воссозданная мной с реально существующей средствами **Blender**, так как на тот момент опыт работы в нем был уже более 2-х лет.

Знакомство с игрой начинается с главного меню, задний фон которого представляет из себя баскетбольный мяч на фоне зала, а перед ним - пункты, плавно меняющие размер в зависимости наведения на них мышью.
Задний фон реагирует на перемещение мыши и плавно вращает мяч и вид зала в зависимости от положения курсора.

![Menu Items](https://drive.google.com/uc?export=view&id=1IGEvH2XHUy1G7QWThypkqAqm1jGEpqZf "Menu Items")

При отсутствии регистрации в игре Вам будет предложено два варианта: ввод логина и пароля, либо создание нового аккаунта, за чем последует кастомизация внешнего вида мяча.

![Ball Customize Menu](https://drive.google.com/uc?export=view&id=1fOq7pnnM5Fz4vFLTxwP1ECW5evDueWUp "Ball Customize Menu")

На площадке игрок может осуществлять броски мяча в корзину, а также управлять музыкальным плеером, который представлен в виде большого дисплея в центре зала и игровым табло ниже.

![Game Environment](https://drive.google.com/uc?export=view&id=1SX4M677NAsN0HNB6TsF_NNvoN6eVpjWv "Game Environment")

# Различия между платформами

В виду того, что проект запланирован для многих платформ, реализация перемещения и бросков на каждой отличается:

- на компьютере перемещение и взаимодействия реализованы на базе FPS-механик: движение на WASD/стрелки, взаимодействия с мячом или дисплеем - кнопки мыши;
- на телефонах управление в процессе разработки, так как на выбор у меня два варианта: тот, что уже реализован в виде той же самой FPS-механики (ссылки на видео оставлю в конце), и тот, что представляет с собой AR-механику, с управлением одним пальцем и переключением функционала специальными UI-элементами;
- в VR всё, в принципе, понятно: передвижение либо телепортацией, либо стиками.

Механика броска:

- на ПК при зажатии ЛКМ и наведении курсора на мяч последний "подбирается в руки", о чем сигнализирует его прозрачность и перемещение в пространстве перед игроком (привет, **Half-Life 2**). Броски осуществляются при отжатии ЛКМ, при этом если мяч направлен в одно из колец запускается механика выбора силы броска, которая подразумевает, что игрок отожмёт кнопку мыши в момент, когда мяч будет зелёным;
- на телефонах сейчас используется такой же метод, однако есть идея управления одним пальцем: сила броска регулируется перемещением большого пальца вертикально, до момента, пока игрок не найдет положение, в котором мяч, опять-таки, становится зелёным;
- в VR - берём мяч и бросаем в кольцо, разве что игра вам немного скорректирует траекторию полёта.

![Ball Throwing](https://drive.google.com/uc?export=view&id=1clcG4vYJ2iKWrASic3BX4Bx0ZIzK9_6x "Ball Throwing")

# Музыка в игре

Что касается музыки, то изначально я хотел написать свой контент и использовать только его, однако за первые месяцы работы над проектом мной был написан один-единственный трек. Тогда и пришла идея о том, что я наберу работы людей, которые будут не против использования их творчества в игровой копии баскетбольного зала Дрейка. Тем более Дрейк очень популярен среди битмейкеров.
Вся музыка и данные о треках и их создателях подгружаются в зависимости от наличия обновлений на сервере. Также имеется счетчик прослушиваний того или иного трека.
Надеюсь, что с данным функционалом в случае популярности проекта у задействованных музыкантов появится больший спрос на их творчество.

![Music Player](https://drive.google.com/uc?export=view&id=1KI9flj9EWYFPwLI43Sxx0TcIhaN-IlIa "Music Player")

# Мультиплеер

В какой-то момент я посчитал, что просто бросать мяч в кольцо будет интересно только тем, кто понимает, что данный зал является копией зала Дрейка - тем, кто скачает приложение только из-за того, что оно случайным образом было ему порекомендовано магазином, захочется какого-то геймплея, и для этого я придумал несколько режимов игры, а чтобы проект было ещё интереснее разрабатывать я приделал к этому всему сетевую составляющую.

![Multiplayer](https://drive.google.com/uc?export=view&id=1HtgalFEK8pOkp1s4VpBOX36aRSORRew_ "Multiplayer")

Как это работает:

- через определенный пункт главного меню игроку предлагается на выбор список уже созданных комнат с определённым режимом игры, а также возможность выбрать игровой режим и запустить свою собственную комнату;
- при присоединении к комнате игрок попадает в лобби, где другие игроки видят друг друга и могут просто пообщаться и побросать мяч, пока ждут готовности всех участников;
- как только все члены лобби готовы игра запускает выбранный режим.

По окончании игры победителю записывается +1 количество побед в определённом режиме, которое в дальнейшем красуется в списке рядом с его никнеймом.

Всё это реализовано при помощи ASP.NET Core, SignalR и MongoDB. Сейчас работаю над передачей файлов для получения клиентами обновлений.