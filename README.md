# Project created for a master's thesis
Master's thesis: Design and architectural patterns for microservices-based applications.

Three distributed systems built for analysing aspects of usage most frequent used architectural patterns:
* Branch architecture pattern
* CQRS
* Event Driven Architecture

Due to lack of time, there are no unit tests. Also, I have not focused on the quality of the code during the implementation.

## General

* Microservices built with ASP.NET Core (service lifecycle, dependency injection, config reading).
* APIGateways use ASP.NET Core WebAPI to provide REST API.
* Data is stored in memory, in ConcurrentDictionary collection at the repository level in code.
* Statistics was being gathered in gRPC interceptors, middlewares and custom classes (RabbitMQ).
* Custom load-balancers (defined on 'loadbalancing' branch) which provide sharding basing on modulo division.

## Branch pattern
Synchronous communication between microservices basing on gRPC. Shared proto files.

## CQRS
Synchronous communication between microservices basing on gRPC. Asynchronous projection between read and write components basing on RabbitMQ. Projection synchronizes whole data objects, not only delta. Very basic Event Sourcing (CommandsInterceptor.cs, cqrs/SharedClasses/Commands/).

## Event-Driven architecture
Asynchronous communication between microservices basing on RabbitMQ. Customized routing which uses C# attributes. Custom awaiter to reconcile asynchronous messaging and synchronous calls to REST Api. (eventdriven/SharedClasses/Messaging/)

## Additional projects
Requester - sending requests with data to system.
DataGenerator - generating initial data.
