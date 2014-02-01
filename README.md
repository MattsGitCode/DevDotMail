DevDotMail
==========

DevDotMail is a simple web app that listens to configurable folders for new emails to be delivered and then parses and indexes them ready for searching.

The main usage of DevDotMail is to accompany the development and testing of other .NET software that requires emails to be sent. Rather than using real email addresses in your software, which you can run out of quickly, you can use any email address and view the email in your browser.

DevDotMail makes use of [NancyFX](http://nancyfx.org) and [OpaqueMail](http://opaquemail.org/)

Get Started
===========

The first thing to do it modify your app.config or web.config to have it drop your emails into a folder instead of firing them off to a real SMTP server.

    <smtp deliveryMethod="specifiedPickupDirectory>
		  <specifiedPickupDirectory>C:\somefolder</specifiedPickupDirectory>
	</smtp>


Next up, get DevDotMail running in IIS, it's a standard affair, nothing special required.

Finally, alter the web.config to tell it where to find your emails

	<appSettings>
		<add key="mailFolder:Emails" value="C:\somefolder" />
	</appSettings>


That's it, send some mails, view the website, voila!


More
====
Multiple pickup folders are supported, simply add multiple keys:

	<appSettings>
		<add key="mailFolder:My First Folder" value="C:\somefolder" />
		<add key="mailFolder:My Second Folder" value="C:\some\other\folder" />
	</appSettings>

In the future there will be an option to filter down by the mail folder name as a top level filter.