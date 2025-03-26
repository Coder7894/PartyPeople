$(document).ready(function () {
    $(".searchable-select").each(function () {
        let $select = $(this);
        let apiUrl = $select.data("search-url");
        let targetInput = $select.data("target-input");
        let requiredCharCount = $select.data("required-char-count") || 0
        let extraParams = $select.data("extra-params");
        let placeholder = $select.data("placeholder");

        if (typeof extraParams === "string") {
            extraParams = JSON.parse(extraParams);
        }

        $select.select2({
            minimumInputLength: requiredCharCount,
            placeholder: placeholder ?? "Search...",
            allowClear: true,
            ajax: {
                url: apiUrl,
                dataType: "json",
                delay: 300,
                data: function (params) {
                    return { ...extraParams, searchTerm: params.term };
                },
                processResults: function (data) {
                    return {
                        results: data.map(function (item) {
                            return { id: item.id, text: item.text };
                        })
                    };
                }
            }
        });

        $select.on("select2:select", function (e) {
            $("#" + targetInput).val(e.params.data.id);
        });
    });
});