# Prepa CH

Utility app to prepare for military service. Translated to English, French, German, and Italian.

## Project organisation
There are two solutions in this VIsual Studio project:
- CHGeoJsonMerger : a utility to merge GeoJSON files into a more usable format for PrepaCH
- Prepa CH : the mobile application described below

## Features
### Rank tester
![rank tester image](/screenshots/rank_tester.jpg)
Practice learning the military ranks of Switzerland!

Has three differen excercise types:
- MCQ for a given rank ensignia (choose rank name)
- MCQ for a given rank name (choose rank ensignia)
- Match rank names to their ensignia
![](/screenshots/rank_tester.mp4)

### Check Convocation Dates
![convocation dates image](/screenshots/convocation_dates.jpg)
Allows users to check the next convocation dates for their unit. A reverse engineering of [this site](https://www.vtg.admin.ch/fr/mon-service-militaire/dates-de-convocation.html) that provides the same service.

### Check Range Dates
![range dates image](/screenshots/range_dates_4.jpg)
Allows users to check range dates for obligatory shooting, searching by program type, weapon type, canton, and date. It is a reverse engineering of the [website that provides the same service](https://ssv-vva.esport.ch/p2plus/ssv/schiesstageabfragerec.asp?). Additionally, the app allows users to search for the range dates that are closest to them. [There is a newer online service](https://www.sat.admin.ch/search-shooting-days), but it doesn't provide a functionality to search by distance.

### Item Checklist
![checklist image](/screenshots/item_checklist.jpg)
A simple checklist of all the items needed when entering service. Saves state in preferences.