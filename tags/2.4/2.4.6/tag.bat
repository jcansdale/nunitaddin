set version=%1
set baseversion=2.4
set message="Tagging version %version%"
set tagroot=https://nunitaddin.svn.sourceforge.net/svnroot/nunitaddin/tags/%baseversion%

svn copy . %tagroot%/%version% -m %message%
svn del %tagroot%/current -m %message%
svn copy %tagroot%/%version% %tagroot%/current -m %message%
