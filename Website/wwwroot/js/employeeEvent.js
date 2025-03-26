$(document).ready(function () {

    $(document).on('click', '.deleteEmployeeBtn', function () {
        var employeeId = $(this).data('id'); 
        deleteEmployeeEvent(employeeId);
    });

    window.createEmployeeEvent = function () {
        var eventId = $('input[name="EventId"]').val();
        var employeeId = $('#EmployeeId').val();

        if (!employeeId) {
            alert('Please select an employee');
            return;
        }

        $.ajax({
            url: '/EmployeeEvent/Create',
            method: 'POST',
            data: { EmployeeEvent: { EventId: eventId, EmployeeId: employeeId } },
            success: function (response) {
                if (response.success) {
                    $('#empty-message').remove()
                    var newEmployeeHtml = `
                    <tr id="employeeEvent-${response.data.id}">
                        <td>${response.data.employeeFirstName} ${response.data.employeeLastName}</td>
                        <td>${response.data.employeeFavouriteDrink ? response.data.employeeFavouriteDrink : "Not Specified"}</td>
                        <td class="text-end">
                            <button class="btn btn-danger btn-sm deleteEmployeeBtn" data-id="${response.data.id}">Delete</button>
                         </td>
                    </tr>
                    `;
                    $('#employeeEventList').append(newEmployeeHtml);
                    $('#employeeSearch').val('').trigger('change');
                    $('#EmployeeId').val("")
                    $('#createEmployeeEventBtn').prop("disabled", response.data.eventIsAtCapacity ? "true" : "false")
                } else {
                    alert('Failed to add employee to event');
                }
            },
            error: function () {
                alert('Error while adding employee');
            }
        });
    }

    function deleteEmployeeEvent(id) {
        $.ajax({
            url: '/EmployeeEvent/Delete',
            url: '/EmployeeEvent/Delete',
            method: 'POST',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    $('#employeeEvent-' + id).remove();
                    $('#createEmployeeEventBtn').prop("disabled", false);
                    if ($('#employeeEventList').children().length === 0) {
                        $('#employeeEventList').append(`
                            <tr id="empty-message">
                                <td>This event has no employees attending</td>
                                <td></td>
                                <td></td>
                            </tr>
                        `)
                    }
                } else {
                    alert('Failed to delete employee from event');
                }
            },
            error: function () {
                alert('Error while deleting employee');
            }
        });
    }
});