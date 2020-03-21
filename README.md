# TvHeadendCli
<h2>TV-Browser -> Aufnahmesteuerung -> Interface for TvHeadend</h2>

<p><a href="https://tvheadend.org/" target="_blank">TvHeadend</a> is a very well working solution for recording tv. It has a quite good web interface.
But I prefere <a href="https://www.tvbrowser.org/" target="_blank">TV-Browser</a> for searching the tv program. So I was looking for a way to schedule TvHeadend recordings with TV-Browser.</p>

<p>I've found no solution but a number of posts of users who where looking for the same. Then I realised that there is a <a href="https://github.com/dave-p/TVH-API-docs/wiki" target="_blank">rest api for TvHeadend</a>! Thanks to dave-p!</p>

<p>Here is a tool to close this gap for Windows. It works with the TV-Browser plugin <a href="https://wiki.tvbrowser.org/index.php/Aufnahmesteuerung" target="_blank">Aufnahmesteuerung</a>.</p>

<p>It is recommended to use TvHeadendGui.exe to create the parameters for Aufnahmesteuerung. None of the tools needs to be installed, they work by copy&paste in any folder with normal user rights. .Net Framework 4.7.2 or higher is required.</p>

<p>A tutorial for setting up the tool with TV-Browser you will find here: <a href="https://github.com/ChrWieg/TvHeadendCli/blob/master/Docs/HowToSetupTvBrowser.docx" target="_blank">docx</a> or here <a href="https://github.com/ChrWieg/TvHeadendCli/blob/master/Docs/HowToSetupTvBrowser.pdf" target="_blank">pdf</a>.</p>

<p>March, 21st 2020: Stability improved, parameters SubTitle, Description and Comment are optinal now. But it makes sense to use all parameters. You will find the Binaries <a href="https://github.com/ChrWieg/TvHeadendCli/tree/master/Binaries" target="_blank">here</a>.</p>
