<24July13> 

refer reference
>extract RefLib
>inside lib file
>open a zxing.wp8.0 using xpress
>go to xpress> project file>right click > add reference
>select solution and u will see zxing.wp8.0
>check it , clean and rebuild whole project
done

added scanner
added friend list
added facebook
created Page folder to organize file
done but buggy> bug, if click fb  first , then it will empty all field when come bacl
http://www.nuget.org/packages/Facebook
>Install-Package Facebook
>Install-Package Facebook.Client -pre

replace friendlistitem class by people class

<22July13> Change theme (comment theme in app.xaml)
Theme manager
>http://www.nuget.org/packages/PhoneThemeManager
>Install-Package PhoneThemeManager
>add in this code to change to white theme
->ThemeManager.ToLightTheme();

Done 2 Request on 21July

<21July13> YinSee help to debug
WebBrowser can cache
QRCodeScanner working
request to change map to hybric
request remove "backToHome button" in indoorPage
https://www.facebook.com/groups/barcamppenang/permalink/616009488422694/
https://itunes.apple.com/my/app/barcamp-penang/id665652007?mt=8
https://play.google.com/store/apps/details?id=com.barcamppenang2013&hl=en

<12July13> Fix minor error
v>fullscreen push pin cannot minimize
v>collapse no internet agenda page
>no internet messagebox

<10July13>Added countdown, homemap

fail route
fail cache ie

declare in the XAML the following namespace to use toolkit in XAML	
> xmlns:toolkit=”clr-namespace:Microsoft.Phone.Maps.Toolkit;assembly=Microsoft.Phone.Controls.Toolkit”

Need to include when doing GPS
>CAPABILITIES ID_CAP_LOCATION

Install to aid map pinpoint
>http://nuget.org/packages/WPToolkit/
>Install-Package WPtoolkit

WebBrowser
>SupportedOrientations property to Portrait only 

<26June13> New Design, recode for WP8

Need to include when doing map
>CAPABILITIES ID_CAP_MAP

Install when doing QRcode
>http://nuget.org/packages/ZXing.Net
>Install-Package ZXing.Net

Passing Param
>http://www.geekchamp.com/tips/how-to-pass-data-between-pages-in-windows-phone-alternatives
Install-Package WPtoolkit
Integrate fb
Install-Package Facebook -Pre

<Overall>
Splash Screen not included yet
#DONE# 
HomePage, ProfilePage<QR>, EditProfilePage
#NotDone# 
MapFullscreen not included,
IndoorMap not done
Friend list,Agenda

http://www.developer.nokia.com/Community/Wiki/How_to_use_Facebook_Graph_API_in_Windows_Phone
http://social.msdn.microsoft.com/Forums/wpapps/en-US/5057b3dd-6e87-49dc-a138-d9c2b18378ad/facebook-api-windows-phone-8
<22May13> Draft created

Time&Venue Page
> Sample code on how to do countdown was put in XAML
> Dont know how to put Application Bar "Navigate" only visible at current page

Profile Page
> QR Code image put but cannot see
> Dont khow how to make Sponsor scrollable

Application Bar
> empty function created at MainPage.xaml.cs

App Icon, Splash Screen, Panorama Image
> waiting official icon and images from organizer

#####
Why you so mean!
Got ah
Just give me a reason
Stay
Everything has changed
Heart attack
Come and get it