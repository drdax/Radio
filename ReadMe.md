# Radio 2 #
Interneta raidstaciju uztvērējs, kurš atbalsta MP3 plūsmas caur HTTP, MMS, ShoutCast un Wowza M3U. Attēlo raidījumu sarakstu un ļauj regulēt skaļumu.

Komplektā nāk visi Rīgas kanāli, BBC pamatkanāli, Itālijas (Rai), Krievijas, Latvijas un Ukrainas (НРКУ) valsts radio, Eirodziesmas, Eiroziņas vairākās valodās, Ретро FM, ХiтFM un daudzējādie 101.ru kanāli.
Noformējums iedvesmots ar VEF 202, kuru jaunībā mēdza klausīties autors. Kanālu izvēli noteica autora, viņa draugu un radu gaume, kā arī tieksme pēc pilnības.

## Klausītājiem ##
**Uzstādīšanai** atpako `Radio2.zip` uz izvēlēto mapi, piemēram, `C:\Program Files\Radio`. Programmas darbība nebūs traucēta, ja mapē nebūs ieraksta tiesības. Komplekta sastāvs:

* Radio.exe – raidstaciju uztvērējs, šo failu vēlams piespraust uzdevumu joslai.
* Radio.exe.config – sākotnējie iestatījumi. Iekļauts, lai zinātu, kā tos mainīt (ne visu iespējams veikt lietotāja saskarnē).
* Radio.xml – Rīgas kanāli, kuriem nāk līdzi tikai logotips. Šeit rakstāmi kanāli, kuriem nav raidījumu saraksta.
* NAudio.dll – bibliotēka, kura palīdz atskaņot MP3. Tā nav nepieciešama MMS kanāliem, piemēram, BBC.
* *.Station.dll – raidstacijas ar vienu vai vairākiem kanāliem, kuriem pieejams noformējums un raidījumu saraksts.
* 101.xml – 101.ru kanālu adreses. Lielākā to daļa ir aizkomentētas, lai klausītāji paši izvēlētos tīkamās.

Darbībai nepieciešams .NET Framework 4.0 Client Profile, Windows versija sākot ar XP, der arī Server paveidi.
Zem Windows 7 un 8 pirmo reizi palaižot programmu no pieejamiem kanāliem tiek izveidots saīšņu saraksts. Iepriekšējo operētājsistēmu lietotājiem nāksies parūpēties par saīsnēm uz mīļotajām stacijām pašiem, piemēram, `Radio.exe retro.3` ieslēdz Ретро FM Rīgas versiju.

Ja palaišanas brīdī parādās ugunsmūra brīdinājums, tad Radio.exe ir aizliedzami ienākošie savienojumi (jo tādi nav paredzēti) un atļaujami izejošie.
Pēdējais atskaņotais kanāls tiek iegaumēts, tāpēc nākošajā reizē var laist, nelietojot saīšņu sarakstu.
Saīšņu saraksts ir tīrāms ar operētājsistēmas standartlīdzekļiem (kontekstizvēlnē nospiežot *Noņemt no šī saraksta*), tad nevēlamie kanāli tiks saglabāti konfigurācijas failā.

Lai **atinstalētu** programmu, ja apnikuši visi kanāli, jādzēš atpakotās mapes saturu un mapi `C:\Users\{lietotāja vārds}\AppData\Local\Dr.Dax_Labs\Radio.exe_Url_{burti un cipari}` (šī mape jādzēš arī, ja palaišanas brīdī saņem paziņojumu *Configuration system failed to initialize*).

## Izstrādātājiem ##
Autors izmanto Visual Studio Express 2012 for Windows Desktop. Professional un 2010 versijām arī jāspēj strādāt ar šo projektu. Iespējams salikt projektu no komandrindas ar MsBuild palīdzību. NAudio bibliotēka piesaistīta ar NuGet starpniecību.

**Stacija** parasti pārstāv vienu raidošo uzņēmumu, piemēram, valsts radiokompāniju vai mēdiju grupu. Stacijas glabājas atsevišķās bibliotēkās ar paplašinājumu `.Station.dll`. Radio uztvērējs ielādē visus šādus failus palaišanas brīdī.
Stacija ir klase, kura manto no `Station` klases un kurai jābūt `Export` atribūtam par to pašu klasi. Katrā bibliotēkā jābūt tieši vienai stacijai, jo bibliotēkas, nevis klases, nosaukums tiek lietots, lai atšķirtu kanālus.

Stacijas konstruktorā tiek noteikti visu tās kanālu numuri, nosaukumi un piesaistītās ikonas. Ja stacijas kanāliem ir kopīga laika josla, to arī norāda konstruktorā. Konstruktorā nedrīkst būt kods, kurš neattiecas uz minēto uzdevumu, jo visu staciju instances tiek veidotas programmas palaišanas brīdī.
Kanālu numerāciju nosaka stacijas autors, katrai stacijai teorētiski var būt kanāli ar numuriem no 0 līdz 255, bet praksē tos ierobežo saīšņu saraksta garums, kuru nosaka lietotājs.

Katrai stacijai ir metode, kura atgriež kanālu instances. **Kanāliem** ir divas pamatklases, `MmsChannel`, plūsmām, kuras atskaņo WPF `MediaPlayer`, un `HttpChannel` (un tā mantinieks `IcyHttpChannel`) visām pārējām plūsmām. Drīkst mantot arī no `Channel`, bet esošajās stacijās pietiek ar minētajām trim.
Katram kanālam obligāta ir tā plūsmas adrese. Papildus var norādīt kanāla logotipu (bet sarakstā – ikonu), studijas laika joslu (lai klausītājs spētu novērtēt raidījumu vadītāju nosauktos laikus), raidījumu saraksta avotu un programmas loga noformējumu.
Kanāliem ir jāspēj automātiski atjaunot atskaņošanu, ja notikusi kāda kļūme, raidījumu sarakstam šāda iespēja pagaidām nav paredzēta. Diemžēl HTTP kanāls retos gadījumos nepareizi nosaka MP3 plūsmas parametrus, kas var izraisīt lēnāku vai ātrāku atskaņošanu.

**Raidījumu saraksts** var nākt no ShoutCast plūsmas (pamatklase `IcyGuide` un tās vienkāršākā gadījuma realizācija `SimpleIcyGuide`), biežas servera taujāšanas (`PollingGuide`, piemēram, eiroziņām, vai `TimedGuide`, kā tas ir 101.ru) vai retas (vienreiz diennaktī) servera taujāšanas (`ListedGuide`, kuras piemēri ir visām valsts stacijām).
Raidījumu sarakstu klases var kombinēt, piemēram, lai attēlotu dienas raidījumu, ja nav zināma pašreizējā dziesma, kā tas ir radio NABA un SWH.

Raidījumu saraksts no klausītāja viedokļa ir iepriekšējais, pašreizējais un gaidāmais (ne vienmēr nākamais) raidījumi, par kuriem zināms sākuma un beigu laiks, nosaukums un apraksts (skatīt klasi `Broadcast`). Visbiežāk nosaukums atbilst dziesmas nosaukumam, bet aprakstā ir izpildītājs. Beigu laiks var būt fiktīvs, ja sarakstā netiek rādīts raidījuma ilgums.

**Noformējumu** veido ar klases `Brand` konstruktoru, kur norāda krāsas un otas loga elementiem. Otas var būt gan vienkārši gradienti, gan attēli, bet nekas mainīgs. Katram kanālam var piešķirt savu ikonu; stacijai vēlams, lai būtu vismaz viena ikona. Vairākas ikonas, piemēram, Latvijas radio, pievienotas kā *res* fails, kurš veidots ar bezmaksas rīku [ResEdit](http://www.resedit.net/). Šajā rīkā arī jānorāda versijas informācija. Ja *res* failā lieto ikonu nosaukumus, tad noskaidrojot to kārtas numurus ir jāņem vērā, ka tās tiek kārtotas alfabēta secībā.
Kanālu logotipi (PNG faili) jāpievieno ar Build Action Resource un jāizgūst ar stacijas metodi `GetResourceImage`. Vēlams logotipa izmēram nepārsniegt 192x152 pikseļus. Attēla malu garumiem jādalās ar divi bez atlikuma, lai tas saglabātu skaidrību.

## Autortiesības ##
Koda autors ir Dr.Dax (Staņislavs Klaušs), ja komentāros nav minēts savādāk, un viņš atļauj to brīvi izmantot citos projektos. Visi grafiskie faili pieder Dr.Dax vai raidstacijām un tos aizliegts izmantot citos projektos vai izplatīt atsevišķi no Radio.

## Radio versijas ##
Pilnīga savietojamība netiek nodrošināta, bet pastāv iezīmju mantojamība.

* 1.0 Viena XML stacija ar `MediaElement` uztvērēja lomā.
* 2.0 Vairākas stacijas ar raidījumu sarakstiem un savu noformējumu. MP3 plūsmošanas paveidu atbalsts.