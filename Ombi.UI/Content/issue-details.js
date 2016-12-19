var base = $('#baseUrl').text();


// Note Modal click
$(".theNoteSaveButton").click(function (e) {
    var comment = $("#noteArea").val();
    e.preventDefault();

    var $form = $("#noteForm");
    var data = $form.serialize();


    $.ajax({
        type: $form.prop("method"),
        url: $form.prop("action"),
        data: data,
        dataType: "json",
        success: function (response) {
            if (checkJsonResponse(response)) {
                location.reload();
            }
        },
        error: function (e) {
            console.log(e);
            generateNotify("Something went wrong!", "danger");
        }
    });
});
// Update the note modal
$('#noteModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget); // Button that triggered the modal
    var id = button.data('identifier'); // Extract info from data-* attributes

    var issue = button.data('issue');
    var modal = $(this);
    modal.find('.theNoteSaveButton').val(id); // Add ID to the button
    var requestField = modal.find('.noteId');
    requestField.val(id);  // Add ID to the hidden field

    var noteType = modal.find('.issue');

    noteType.val(issue);


});


$('.delete').click(function(e) {
    e.preventDefault();
    var url = createBaseUrl(base, "/issues");
    var $form = $("#removeForm");

        $.ajax({
            type: $form.prop("method"),
            url: $form.prop("action"),
            data: $form.serialize(),
            dataType: "json",
            success: function (response) {
                if (checkJsonResponse(response)) {                    window.location.replace(url);                }
            },
            error: function (e) {
                console.log(e);
                generateNotify("Something went wrong!", "danger");
            }
        });
    });




