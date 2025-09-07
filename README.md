# ObsScoreboard

Obs'te Puan Tablosu göstermek yerel sunucu tabanlı bir uygulama.

## 🛠️ Kurulum
[https://github.com/makifgokce/obs-scoreboard/releases](https://github.com/makifgokce/obs-scoreboard/releases) adresinden yerel sunucuyu indirin.
`ScoreboardServer.zip` dosyasını çıkartın.
`ScoreboardServer.exe` dosyasını çalıştırın.
### Alternatif olarak Python server
Bilgisayarınızda python kurulu değilse pyhon indirip kurun.
[https://github.com/makifgokce/obs-scoreboard/archive/refs/heads/main.zip](https://github.com/makifgokce/obs-scoreboard/archive/refs/heads/main.zip) projeyin indirip içerisindekini istediğiniz bir yere çıkarın.

> Dosyaları çıkardığınız konumda `PyServer` klasörü içerisinde `Komut İstemi`'ni açın.

> Komut İsteminde `cd <PyServer Klasörünün konumu>` komutuyla `PyServer` klasörüne giriş yapabilirsiniz.

> Örnek:
```
cd C:\Users\<UserName>\Desktop\obs-scoreboard-main\PyServer
```

> Aşağıdaki komutu çalıştırın.
```
pip install -r requirements.txt
```
> Sunucuyu çalıştırmak için aşağıdaki komutu girin.
```
python server.py
```

>Obs'te Sahneye Browser ekleyin.
### Puan Tablosu için:
>URL kısmına aşağıdaki linki girin.
```
https://makifgokce.github.io/obs-scoreboard/leaderboard
```

### Takım Skoru için:
>URL kısmına aşağıdaki linki girin.
```
https://makifgokce.github.io/obs-scoreboard/teamscore
```
### Sayaç için:
>URL kısmına aşağıdaki linki girin.
```
https://makifgokce.github.io/obs-scoreboard/counter
```
### Kontrol Paneli
>Kontrol paneli isterseniz [https://makifgokce.github.io/obs-scoreboard/](https://makifgokce.github.io/obs-scoreboard/) adresini tarayıcınızda açarak yapabilirsiniz.

veya

>Obs menüsünde `Docks` > `Custom Browser Docks` a tıklayın.
`Dock Name` Kısmına istediğiniz bir isim girebilirsiniz.
`URL` kısmına aşağıdakini girin.

```
https://makifgokce.github.io/obs-scoreboard/
```

`Apply` a tıklayarak artık OBS içerisinden de kontrol edebilirsiniz!