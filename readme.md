# M1nforum

M1nforum: a play project in the disguise of an oldschool forum.  This is not production code.  The techniques and code is for fun and this is not a reference architecture.

## Description

M1nforum is a fun exercise - nothing more.  

There is a live version on the free tier in azure here:  [minforum.azurewebsites.net](https://m1nforum.azurewebsites.net/).  This is running on the F1 app service plan so who knows how long it will be up and available.  I'm curious to see what you get with the free tier in Azure. (60 CPU minutes / day).  And, since its a windows host and all the data is in memory, I am sure it is memory starved.  Once it loads, I am sure it will be fast.  :)

There is also a problem with the seed function.  I'm not sure what it is, but its not consistent.

There is a focus on minimalism and speed
- Custom view engine compiles html to c# at build time
- The custom view engine writes strings directly to the body stream.
- The data is from an xml file on disk but it is all cached.  
- There is no dependency on a database.
- There is use of caching headers in every http handler method.
- The css is from the smallest css library I could find and its awesome [mincss.com](https://mincss.com/)
- There is no javascript.
- MapGet is used - no razorpages or mvc or razor.
- Everything is minimal and fast
- Lots of things are done the manual/old school way.  For example, use of httpContext, custom view engine, xml data store.  There are better, tested libraries for all of this functionality.

It is not finished.  
- No write capability - cant create any content via the UI yet
- No user management - cant even create an account
- No search
- No database
- No spam protection
- Caching headers are suspect
- No tests

You get the idea - I was messing around and had a blast.  Feel free to ignore.

### Data store

The data is stored in xml files on disk.  The data is cached in memory for fast reads.

### View Engine

The console app is a view compiler.  It compiles the html templates into c# using the [John Resig Micro-Templating Pattern](https://johnresig.com/blog/javascript-micro-templating/), but in c#.

This view engine is not the most fun developer experience.  To rebuilt the view you have to rebuild the app.  And, errors are tedious to find.  I think they are fast, though.  Keep the views simple and it is easier.

## Getting Started

If you want to run this website, load it up in visual studio and hit F5.  There are no dependencies on anything besides .net core 6.

### Dependencies

* asp.net core 6

### Installing

There is a seed function so if you delete the data files, they will regenerate with random data.  This seems buggy so delete and rebuild until you get a bunch of comments.  It is bad practice, but I added the input files to this git repo.  Should probably delete those and let them generate every time.

There is a seed method and most of the content or title fields are random words.

### Odd Info

* This uses an html file and string parsing for a custom view engine
* The template is copied to the bin folder on build of the soluution - old school with a build event and a .cmd file.

## Help

Dont use this code for prod apps.  There are interesting code bits but this whole app is not production ready, hasn't been tested and I am not sure what the data capacity is.  Its cached and well done but still, how many records do you want to store in an xml file on disk next to the web app?

## Authors

M1nus - m1nus49961 at gmail - I don't frequently check this email.

## Version History

* 0.0.0.0.0.1
    * initial commit

## License

This project is licensed under the MIT License - see the LICENSE.txt file for details

## Acknowledgments

Inspiration, code snippets, etc.
* Inspired by [orange forum](https://github.com/s-gv/orangeforum) 
> This is an attempt to combat the website obesity crisis and reduce web bloat.
* [mincss](http://mincss.com/) - we rarely need much more than this
* [Free Azure App Service](https://azure.microsoft.com/en-in/products/app-service/)
* Visual Studio - you could write this all in vscode.  I use visual studio.
* [Mads Web Compiler](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.WebCompiler) - you could just use npm but I am lazy so this minifies my css for me in visual studio without effort.
