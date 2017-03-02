var errorContainerMap = {
    "#ingredientsTable": "#ingredientErrors",
    "#recipe": "#recipeErrors",
    "#procedureList": "#procedureErrors"
};

$(document).ready(function () {

    $("#ingredientsTable tbody").sortable({
        revert: true
    });
    $("#procedureList").sortable({
        revert:true
    });

    autosize($('textarea'));

    $("#procedureList li textarea").on('blur', removeStepIfEmpty);

    parseValidator($('form'));
});

function parseValidator(form) {
    form.data('validator', null);
    $.validator.unobtrusive.parse(form);

    // copy any ingredient validation errors to the summary div
    form.data("validator").settings.onfocusout = function (element) { $(element).valid(); };
    form.data("validator").settings.showErrors = function (errorMap, errorList) {
        this.defaultShowErrors();
        dispatchErrorsToContainers();
    };
}

function dispatchErrorsToContainers() {
    for (var key in errorContainerMap) {
        if (errorContainerMap.hasOwnProperty(key)) {
            var errors = errorContainerMap[key];
            $errorContainer = $(errors);
            $errorContainer.empty();
            $(".field-validation-error span", $(key))
                .clone()
                .appendTo(errors)
                .wrap("<div>");
            if ($errorContainer.children('div').length) {
                $errorContainer.show();
            } else {
                $errorContainer.hide();
            }
        }
    }
}

//http://stackoverflow.com/questions/28019793/submit-same-partial-view-called-multiple-times-data-to-controller/28081308#28081308
//http://stackoverflow.com/questions/29837547/set-class-validation-for-dynamic-textbox-in-a-table/29838689#29838689

function addRow() {
    var clone = $(".templateRow").clone();
    clone.removeClass("templateRow")

    var index = (new Date()).getTime();
    clone.html($(clone).html().replace(/#/g, index));

    $('#ingredientsTable tbody').append(clone);
    parseValidator($('form'));
    clone.show();
}

function deleteRow(event) {
    $(event.target).closest('tr').remove();
}

function addStep() {
    var clone = $(".templateStep").clone();
    clone.removeClass("templateStep")

    var index = (new Date()).getTime();
    clone.html($(clone).html().replace(/#/g, index));
    clone.find('textarea').on('blur', removeStepIfEmpty);

    $('#procedureList').append(clone);
    clone.show();
    autosize(clone.find('textarea'));
    parseValidator($('form'));
}

function removeStepIfEmpty(event) {
    if (event.target.value == "") {
        $(event.target).closest('li').remove();
    }
}
