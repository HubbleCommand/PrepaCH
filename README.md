# Prepa CH

Utility app to prepare for military service. Translated to English, French, German, and Italian.

## Project organisation
There are two solutions in this VIsual Studio project:
- CHGeoJsonMerger : a utility to merge GeoJSON files into a more usable format for PrepaCH
- Prepa CH : the mobile application described below

## Features
### Rank tester
<img src="/screenshots/rank_tester.jpg" alt="rank tester image" width="200"> 
Practice learning the military ranks of Switzerland!

Has three differen excercise types:
- MCQ for a given rank ensignia (choose rank name)
- MCQ for a given rank name (choose rank ensignia)
- Match rank names to their ensignia

<video width="250">
  <source src="/screenshots/rank_tester.mp4" type="video/mp4" />
</video>

### Check Convocation Dates
<img src="/screenshots/convocation_dates.jpg" alt="convocation dates image" width="200"> 
Allows users to check the next convocation dates for their unit. A reverse engineering of [this site](https://www.vtg.admin.ch/fr/mon-service-militaire/dates-de-convocation.html) that provides the same service.

### Check Range Dates
<img src="/screenshots/range_dates_4.jpg" alt="range dates image" width="200"> 
Allows users to check range dates for obligatory shooting, searching by program type, weapon type, canton, and date. It is a reverse engineering of the [website that provides the same service](https://ssv-vva.esport.ch/p2plus/ssv/schiesstageabfragerec.asp?). Additionally, the app allows users to search for the range dates that are closest to them. [There is a newer online service](https://www.sat.admin.ch/search-shooting-days), but it doesn't provide a functionality to search by distance.

### Item Checklist
<img src="/screenshots/item_checklist.jpg" alt="checklist image" width="200"> 
A simple checklist of all the items needed when entering service. Saves state in preferences.