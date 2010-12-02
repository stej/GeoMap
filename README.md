GeoMap
=============

I just wanted to try F# for some real project. So, here it is.

Purpose
-------

GeoMap is application for people from Czech Republic interested in geocaching. I collected info about all the caches in the Czech Republic and put it into a SQLite database.

When you run the application, all the caches are displayed. You can zoom in/out to see the details. You can optionally export caches that are located in the visible region. Currently, the caches are exported to file `<bindirectory>\exported.db` -- it is not possible to pick custom name.

It is possible to filter the caches by type, size and possibly by keywords, but this is not fully supported right now.

GMap.NET && SQLite
------

The application uses [GMap.NET](http://greatmaps.codeplex.com/). It is a WinForm control that displays map of your choice (Google, Bing, ... ) and caches it. So, later when you open the application again, the map images are fetched from cache if possible. That means less troubles with internet connection.

How to run it
-------------

You run the application very easily: 

    ps> cd GeoMaps
    ps> .\compile.ps1
    ps> bin\byscript\GeoMap.exe .\caches.db
    
You can also download the binary files and data in Downloads section. Or click on the [direct link](https://github.com/downloads/stej/GeoMap/GeoMap-bin.zip). Then unzip it and simply run

    ps> cd GeoMaps
    ps> GeoMap.exe caches.db
    
Data
----

Caches are currently up to date. However, new caches are being added for sure while you are reading this readme ;) That's the limitation of the application -- although I might update the data, it won't be 100% reliable.