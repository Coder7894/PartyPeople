$(document).ready(function () {
    $(".list-widget").each(function () {
        let list = $(this);
        let apiUrl = list.data("api-url");
        let redirectUrl = list.data("redirect-url");
        console.log("here", apiUrl)
        $.ajax({
            url: apiUrl,
            type: "GET",
            success: function (data) {
                list.empty();

                if (data.length === 0) {
                    list.append("Nothing to show ")
                }

                $.each(data, function (index, item) {
                    let listItem = $(`
                        <li>
                            <a href="${redirectUrl}/${item.id}">${item.text}</a>
                        </li>
                        `)
                    list.append(listItem);
                });
            },
            error: function () {
                list.html("<li>Error loading items</li>");
            }
        });
    })
})