Posharp Aspect Library
======================

... is a bunch of aspects used to clean code from various cross-cutting concerns. It's based on PostSharp AOP. The library can be used with .NET 3.5 and .NET 4.0. 

PostSharp AOP can be downloaded from http://www.sharpcrafters.com or just use the corresponding Nuget package. There is a free version which is very powerful and should be able to be used in most of your applications.

Included Aspects
----------------
 * LogCalls - Logging of entering and exciting the current method. Also logs exceptions.
 * TimeProfile - Metering the method execution time and writing it to log4net loggers.
 * ParameterNotNullCheck - Check if the given parameter is null, if null throws.
 * RetryOnError - Will retry the method execution, if there was an exception.
 * HandleException - Handles an exception occur in the current method. (Allows supression)
 * various PerformanceCounter
 * NotifyPropertyChanged (copy from PostSharp samples) - Implements the NotifyPropertyChanged for every property.
 * Threading (copy from PostSharp samples) 
 * Caching (copy from PostSharp samples)
 * AddDataContract (copy from PostSharp samples) - Applies the DataContract Attribute to each class (WCF)
 
Used Libraries
--------------
* PostSharp - http://www.sharpcrafters.com 
* Log4Net - http://log4net.apache.org
* NUnit - http://www.nunit.org
* Moq - http://code.google.com/p/moq/
* Nuget - http://www.nuget.org