# MP3 Encoder GUI
This application gives You the ability to easily encode waveform audio formats into MP3 using a Graphical User Interface (GUI).
The program is written in [C#][] and [XAML][] ([WPF][]), and uses the command line version of the [LAME MP3 Encoder][] in order to work.

[C#]: http://wikipedia.org/wiki/C_Sharp_%28programming_language%29
[XAML]: http://wikipedia.org/wiki/XAML
[WPF]: http://wikipedia.org/wiki/Windows_Presentation_Foundation
[LAME MP3 Encoder]: http://lame.sourceforge.net/

## Usage requirement
To avoid legal problems, the LAME Encoder's assemblies are not included in the project.
Please obtain (compile or download) _lame.exe,_ and put it into the output directory's _\lame_ folder.

_The application is tested to work flawlessly with LAME v3.99.5._

## About versioning
    <Major>.<Minor>.<Build>.<Revision>

  * Major releases feature new functions that older versions didn't have.
  * Minor version increments happen when there are significant performance or UI improvements.
  * Build versions are increased with each successful commit which has changes to the code, and can be built.
  * Revisions are quick fixes for builds that don't work as expected or need code clean-up, but have already been committed and pushed.
