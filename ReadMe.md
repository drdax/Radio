# Dr.Dax Radio 2.1
Interneta raidstaciju uztvērējs, kurš atbalsta MP3 plūsmas caur HTTP, MMS, ShoutCast un Wowza M3U. Attēlo raidījumu sarakstu un ļauj regulēt skaļumu. Vairākiem kanāliem ļauj skatīties studijas tiešraidi un uzzināt skanējušās dziesmas.

Komplektā nāk visi Rīgas kanāli, BBC pamatkanāli, Itālijas (Rai), Krievijas (ВГТРК) un Ukrainas (НРКУ) valsts radio, Eirodziesmas, Eiroziņas vairākās valodās, ЭХО Москвы, Ретро FM, Тавр un 101.ru daudzējādie kanāli.
Ikonas un radioaparāta izskats iedvesmots ar VEF 202, kuru jaunībā mēdza klausīties autors. Kanālu izvēli noteica autora, viņa draugu un radu gaume, kā arī tieksme pēc pilnības.

## Klausītājiem
**Uzstādīšanai** atpako [Radio21.zip](https://dl.dropbox.com/s/8b118kx438hgedf/Radio21.zip) uz izvēlēto mapi, piemēram, _C:\Program Files\Radio_. Programmas darbība nebūs traucēta, ja mapē nebūs ieraksta tiesības. Komplekta sastāvs:

* Radio.exe – raidstaciju uztvērējs, šo failu jāpiesprauž uzdevumu joslai pilnvērtīgai darbībai.
* Radio.xml – Rīgas un daži citi kanāli, kuriem nāk līdzi tikai logotips. Šeit rakstāmi kanāli, kuriem nav raidījumu saraksta vai tas ir Icy formātā.
	Katru kanālu apraksta viens `Channel` elements sadaļā `Channels`. Tam ir obligāts atribūti `Caption` (sarakstā un virsrakstā attēlojamais nosaukums) un kāds no norādošiem plūsmu (satur tās pilno adresi): `Url` pirmkārt MMS kanāliem, `Icy` ShoutCast kanāliem bez dziesmu saraksta un `IcyGuide` ar dziesmu sarakstu.
 	Neobligātie atribūti ir `Time` (laika joslas nosaukums pulksteņa attēlošanai, iespējamās vērtības skatīt Windows reģistrā _HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones_, noņemot beigās “ Standard Time”), `Homepage` (kanāla mājaslapas adrese) un `LogoId` (logotipa identifikators sadaļā `Logos`).
	Vienam vai vairākiem kanāliem var būt pievienots logotipa attēls, Base64 pārkodēts PNG fails, kuru ievieto elementā `Logo` ar unikālu atribūta `Id` vērtību, uz kuru atsaucās `Channel` elementā.
* NAudio.dll – bibliotēka, kura palīdz atskaņot MP3. Tā nav nepieciešama MMS kanāliem, piemēram, BBC.
* *.Station.dll – raidstacijas ar vienu vai vairākiem kanāliem, kuriem pieejams noformējums un raidījumu saraksts.

Darbībai nepieciešams .NET Framework 4.5.1 vai jaunāks (esošo versiju var pārbaudīt ar [Smallest .NET](http://smallestdotnet.com/) palīdzību), Windows versija sākot ar 7, der arī Server paveidi.
Zem Windows 7 un 8 pirmo reizi palaižot programmu no pieejamiem kanāliem tiek izveidots un parādīts saīšņu saraksts.

Ja palaišanas brīdī parādās ugunsmūra brīdinājums, tad Radio.exe ir aizliedzami ienākošie savienojumi (tādi nav paredzēti no attālinātiem datoriem) un atļaujami izejošie.
Pēdējais atskaņotais kanāls tiek iegaumēts, tāpēc nākošajā reizē var laist, nelietojot saīšņu sarakstu.
Saīšņu saraksts ir tīrāms ar operētājsistēmas standartlīdzekļiem (kontekstizvēlnē nospiežot _Noņemt no šī saraksta_), tad nevēlamie kanāli tiks saglabāti konfigurācijas failā. Tas attiecas arī uz 101.ru un Pieci.lv kanāliem, kurus izvēlas iestatījumu logos.

Lai **atinstalētu** programmu, ja apnikuši visi kanāli, jādzēš atpakotās mapes saturu un mapi `C:\Users\{lietotāja vārds}\AppData\Local\Dr.Dax_Labs\Radio.exe_Url_{burti un cipari}` (šī mape jādzēš arī, ja palaišanas brīdī saņem paziņojumu _Configuration system failed to initialize_).

### Karstie taustiņi
<kbd>G</kbd> Ieslēgt/izslēgt raidījumu sarakstu<br/>
<kbd>T</kbd> Ieslēgt/izslēgt pulksteni ar studijas laiku<br/>
<kbd>M</kbd> Ieslēgt/izslēgt skaņu<br/>
<kbd>↑</kbd> Palielināt skaļumu<br/>
<kbd>↓</kbd> Samazināt skaļumu<br/>
<kbd>C</kbd> Kopēt uz starpliktuvi pašreizējā raidījuma datus<br/>
<kbd>X</kbd> Kopēt iepriekšējā raidījuma datus<br/>
<kbd>H</kbd> Atvērt kanāla mājaslapu<br/>
<kbd>F1</kbd> Atvērt informācija par pašreizējo raidījumu<br/>
<kbd>F2</kbd> Atvērt agrāk skanējušo raidījumu sarakstu<br/>
<kbd>F3</kbd> Atvērt video translāciju no studijas<br/>
<kbd>F4</kbd> Atvērt stacijas iestatījumus

## Izstrādātājiem
Autors izmanto Visual Studio 2013, bet iespējams salikt projektu no komandrindas ar MSBuild palīdzību. [NAudio bibliotēka](http://naudio.codeplex.com/) piesaistīta ar NuGet starpniecību.

**Stacija** parasti pārstāv vienu raidošo uzņēmumu, piemēram, valsts radiokompāniju vai mēdiju grupu. Stacijas glabājas atsevišķās bibliotēkās ar paplašinājumu `.Station.dll`. Radio uztvērējs ielādē visus šādus failus palaišanas brīdī un sarakstā attēlo pēc DLL nosaukumiem alfabēta secībā.
Stacija ir klase, kura manto no `Station` klases vai tās apakšklases. Katrā bibliotēkā (DLL) jābūt tieši vienai stacijai, jo bibliotēkas, nevis klases, nosaukums tiek lietots, lai atšķirtu kanālus.

Stacijas konstruktorā tiek noteikti visu tās kanālu numuri, nosaukumi un piesaistītās ikonas. Kanālu secību nosaka stacija ar pievienošanas kārtību. Ja stacijas kanāliem ir kopīga laika josla, to arī norāda konstruktorā. Konstruktoram nav jāveic citi uzdevumi, lai stacija ātrāk ielādētos programmas palaišanas brīdī.
Kanālu numerāciju nosaka stacijas autors, katrai stacijai teorētiski var būt kanāli ar numuriem no 0 līdz 4294967295 (datu tips `uint`, neoligāti pēc kārtas), bet praksē tos ierobežo saīšņu saraksta garums, kuru nosaka lietotājs.

Katrai stacijai ir metode `GetChannel`, kura atgriež kanālu instances. **Kanāliem** ir divas pamatklases, `UrlChannel`, plūsmām, kuras atskaņo WPF `MediaPlayer`, un `StreamChannel` (tā mantinieki `IcyChannel` un `SegmentedChannel`) visām pārējām plūsmām. Drīkst mantot arī no `Channel`, bet iekļautajās stacijās pietiek ar minētajām trim.
Katram kanālam obligāta ir tā plūsmas adrese. Papildus var norādīt kanāla logotipu (bet sarakstā – ikonu), studijas laika joslu (lai klausītājs spētu novērtēt raidījumu vadītāju nosauktos laikus), raidījumu saraksta avotu un programmas loga noformējumu, kā arī komandu izvēlni.
Kanāliem ir jāspēj automātiski atjaunot atskaņošanu, ja notikusi kāda kļūme, raidījumu sarakstam šāda iespēja pagaidām nav paredzēta.

**Raidījumu saraksts** var nākt no ShoutCast plūsmas (pamatklase `IcyGuide` un tās vienkāršākā gadījuma realizācija `SimpleIcyGuide`), biežas servera taujāšanas (`PollingGuide`, piemēram, eiroziņām, vai `TimedGuide`, kā tas ir 101.ru) vai retas (vienreiz diennaktī) servera taujāšanas (`ListedGuide`, kuras piemēri ir visām valsts stacijām).
Raidījumu sarakstu klases var kombinēt, piemēram, lai attēlotu dienas raidījumu, ja nav zināma pašreizējā dziesma, kā tas ir radio NABA un SWH.

No raidījumu saraksta programmas logā attēlo iepriekšējo, pašreizējo un gaidāmo (ne vienmēr nākamo) raidījumus, par kuriem zināms sākuma un beigu laiks, nosaukums un apraksts (skatīt klasi `Broadcast`). Visbiežāk nosaukums atbilst dziesmas nosaukumam, bet aprakstā ir izpildītājs. Beigu laiks var būt fiktīvs, ja sarakstā netiek rādīts raidījuma ilgums.

**Noformējumu** veido ar klases `Brand` konstruktoru, kuram norāda krāsas un otas loga elementiem. Otas var būt gan vienkārši gradienti, gan attēli, bet nekas mainīgs. Katram kanālam var piešķirt savu ikonu; stacijai vēlams, lai būtu vismaz viena ikona. Vairākas ikonas vienai stacijai, piemēram, Latvijas radio, pievienojamas kā *res* fails, kurš veidots ar bezmaksas rīku [ResEdit](http://www.resedit.net/). Šajā rīkā tad arī jānorāda versijas informācija. Ja *res* failā lieto ikonu nosaukumus nevis numurus, tad noskaidrojot to kārtas numurus ir jāņem vērā, ka tās tiek kārtotas alfabēta secībā.
Kanālu logotipi (PNG faili) jāpievieno ar Build Action _Resource_ un jāizgūst ar stacijas metodi `GetResourceImage`. Vēlams logotipa izmēram nepārsniegt 192x152 pikseļus. Attēla malu garumiem jādalās ar divi bez atlikuma, lai tas saglabātu skaidrību centrējot.

Katram kanālam un raidījumu sarakstam var būt līdz piecām **komandām**, kuras izsaucamas no programmas izvēlnes. Bieži sastopamām komandām pieejamas ikonas (uzskaitījums `MenuIcon`) un tās izsaucamas ar funkcionālo taustiņu palīdzību. Lai iekļautos izvēlnē, jāmantojas no `Menu` klases, konstruktorā jānorāda pieejamās komandas un lietotāja izvēle jāapstrādā metodē `HandleCommand`. Komandu izvēlne tiek padota kanāla vai raidījumu saraksta konstruktoram.
Īpaša komanda ir `Information`, jo to iespējams izsaukt arī ar pogu zem radiouztvērēja sīktēla uzdevumu joslā; pēc tās izsaukšanas ir jāatver pašreizējā raidījuma Tīmekļa lappuse (vai jāpaziņo, ka informācija nav pieejama).

## Autortiesības
Koda autors ir Dr.Dax (Staņislavs Klaušs), ja koda komentāros nav minēts savādāk, un viņš atļauj to brīvi izmantot citos projektos. Visi grafiskie faili pieder Dr.Dax vai raidstacijām un tos aizliegts izmantot citos projektos vai izplatīt atsevišķi no Dr.Dax Radio.

## Radio versijas
Pilnīga savietojamība netiek nodrošināta, bet pastāv iezīmju mantojamība. Versiju numuri mainās reti, tāpēc jāpievērš uzmanība salikšanas datumam, kuru var apskatīt logā _Par Radio_.

* 1.0 – Viena XML stacija ar `MediaElement` uztvērēja lomā.
* 2.0 – Vairākas stacijas ar raidījumu sarakstiem un savu noformējumu. MP3 plūsmošanas paveidu atbalsts.
* 2.1 – Izvēlne ar komandām un iestatījumiem. Stacijas ar maināmu kanālu sarakstu. Video uztveršana no studijām.
