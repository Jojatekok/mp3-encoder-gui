# MP3 Encoder GUI
This application gives You the ability to easily encode waveform audio formats into MP3 using a Graphical User Interface (GUI).
The program is written in [C#][] and [XAML][] ([WPF][]), and uses the command line version of the [LAME MP3 Encoder][] in order to work.

[C#]: http://wikipedia.org/wiki/C_Sharp_%28programming_language%29
[XAML]: http://wikipedia.org/wiki/XAML
[WPF]: http://wikipedia.org/wiki/Windows_Presentation_Foundation
[LAME MP3 Encoder]: http://lame.sourceforge.net/

## Usage requirements
Installing [Microsoft .NET Framework 4.5][] (or higher) is a requirement for both end-users and developers.

To avoid legal problems, the LAME Encoder's assemblies are not included in the project.
Please obtain (compile or download) _lame.exe,_ and put it into the output directory's _"lame"_ folder.

_The application is tested to work flawlessly with LAME v3.99.5._

[Microsoft .NET Framework 4.5]: http://www.microsoft.com/download/details.aspx?id=30653

## About versioning
    <Major>.<Minor>.<Build>.<Revision>

  * Major releases feature brand-new functions which older versions didn't have.
  * Minor version increments happen when there are significant performance, UI, or code improvements.
  * Build versions are increased with each successful commit which has changes to the code, and can be built.
  * Revisions are quick fixes for builds that don't work as expected or need code clean-up, but have already been committed and pushed.
