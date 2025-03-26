# Party People

## Scenario

At Koderly, our employees attend a lot of events throughout the year, and we need an application to track which events our employees will be attending. 
Koderly has started creating an application called PartyPeople. The application is buggy and unfinished, and we need your help to complete it.

### Built With

The PartyPeople application is an ASP.NET Core MVC application, written in C# and targets the .NET 8 Framework. The application uses SQLite for its database and includes the following technologies.

* C#
* .NET 8
* ASP.NET Core
* [Dapper](https://github.com/DapperLib/Dapper)
* [FluentValidation](https://docs.fluentvalidation.net/en/latest/)
* [Bootstrap](https://getbootstrap.com/docs/5.3/getting-started/introduction/)

## Getting Started

### Prerequisites

Development for the PartyPeople application relies on the below software:

* Visual Studio 2022 Community Edition, incl. ASP.NET and Web Development

### Build Steps

To build the PartyPeople application, you need to follow the steps below. 

1. Clone the repository
	```
	git clone https://github.com/Coder7894/PartyPeople.git
	```

2. Open the PartyPeople.sln in Visual Studio

3. Build and Run the Website project in Visual Studio

## Tasks

You only need to spend as much time on the tasks as you feel necessary to demonstrate your capabilities, and you don't need to complete every task. 
We estimate that it could take between four and seven hours to complete all tasks if you choose to do so.

To complete the application, you need to carry out the tasks below. 

### Task 1
The PartyPeople project does not currently build. Can you help figure out why, and resolve the build issues?

#### Task 1 answer	
The Nuget Package Dapper was missing from the project. Adding it resolved all the build issues
	
### Task 2
A bug has been reported that updating events is not working as expected. Can you help by debugging the functionality and resolving the issue?

#### Task 2 answer
I was able ot identify the issue by running the project, creating a couple of events and trying to update one of them. This resulted in an exception being thrown that told me what the issue was.

The Sql in the EventRepository.UpdateAsync() had missing WHERE Clauses in the UPDATE and SELECT section of the query. This meant the query was updating every record in the event table and then trying to return every record. Instead the desired behaviour should be to Update and Retrieve just one Event

Returning every record threw an exception because
		```
        var updatedEvent = await Connection.QuerySingleAsync<Event>(command);
		```
could not map a collection of Events to a single Event object. 

#### Testing
At this point I decided to add Tests to the project to make sure every feature works as intended and to help with future debugging. Unit/Integration tests are usually the first place I look when trying to debug. I also made sure to add a test covering the issue from Task 2.

For simplicities sake I opted to use an in memory database to test on. But in a real project my preference would be to use a locally hosted test database.

I also added my preferred package for Assertions: Shouldly.

### Task 3
Koderly would like to track which employees are attending which events. Can you extend the PartyPeople application to add this functionality?

#### Task 3 answer
I Achieved this by creating a Linking Table called EmployeeEvent and a Repository for it.
 
I made sure to respect the maximum capacity prop on the Event. There is some general checks in the controllers to return errors if a request attempts to go over the capacity. The Edit Event Form will now show a validation error if edditing the maximum capacity below the amount of attendees

The Employee display page was extended to show a list of events that employee has/will attend

The Event display page was extended to manage employees who are attending an event. Here you can search for employees that are not currently attending the event and add them. There is also the ability to remove them if needed. Here the button to Add a new employee will disable itself when the event has reached capacity and undisable itself when a employee is deleted.

The javascript used for this searchable select is generic so if I wanted to add similar functionality to the employee page to manage which events that employee is attending the front end code could be reused. All that would be required is a new endpoint to get Events that the employee is not currently attending.

### Task 4
Koderly would like to track which drinks should be ordered for employees. Can you extend the functionality to allow each employee to optionally specify a ‘Favourite Drink’?

#### Task 4 answer
I have extended the employee create and edit form with a text input for their favourite drink. This data gets stored in a new column in the Employee table and is shown on the employee details page and the Events display page in the attending employees section. This data can be empty if the employee does not wish to specify a favourite drink, doing this will render placeholder text "Not Specified" on the front end. 

### Task 5
Koderly would like to track the five most social employees (i.e. employees who have attended the most events). Can you add a widget to the Home screen to display this information?

#### Task 5 answer
I created a Generic ViewComponent called ListWidget. This view component consumes a title, a url to get its data from and another url to serve as a link if you click on a list item.

I created a new endpoint for getting the data ("EmployeeEvent/GetMostSocialEmployees")

### Task 6
Koderly would like to track upcoming events which have no attendees registered. Can you add a widget to the Home screen to display this information?

#### Task 6 answer
Using the same View Component for task 5 and also creating a new endpoint to get the data into the component, there is now a widget on the home screen to show events with no attendees.

### Polish and Optimisation
I have added indexes to the columns EmployeeId and EventId on the EmployeeEvent table as these columns will be queried regularly.

I have added an index to the StartDateTime Column on the event table as this table is often filtered by property to get only future events.

I have added a computed column called Fullname to the Employee table which is a concatonation of the first and lastname columns and added an index on to it. This is because the the searchable select component used in task 3 searches the employees by their full name.

I have added an empty list state to the attending employee table on the event detail page
I have added an empty list state to the events table on the employee detail page


## Submission
Please commit your work for review by __3 pm__ on __Friday, 13 September 2024__, by completing the steps below.

1. Fork the PartyPeople project
2. Create a Feature Branch (``` git checkout -b feature/{featureName} ```)
3. Commit your changes (```git commit -m 'My Commit Note'```)
4. Push to your branch (```git push origin feature/{featureName}```)
5. Open a Pull Request

## Contact
If you need help with a particular task, or cannot proceed for any reason, please don’t hesitate to contact us by email at [recruitment@koder.ly](mailto:recruitment@koder.ly).