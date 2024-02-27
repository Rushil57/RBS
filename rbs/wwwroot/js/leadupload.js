$(document).ready(function () {
    ChcekCookie();

    $("#spanClockinout").load("/clockinout.html");

    checkAgent();
    $("#upload").click(function (e) {
        $("#loader").show();
        $("#upload").hide();
        uploadleads();
    });
});

function uploadleads() {
    var url = "/api/LeadUpload/";
    
    var formData = new FormData();
    formData.append("file", $("#leaduploadfile")[0].files[0]);
   

    $("#errorMessage").hide();
    $("#successMsg").hide();
    $("#waitMsg").show();

    $.ajax({
        url: url,
        type: "POST",
        data: formData,
        processData: false,  // tell jQuery not to process the data
        contentType: false,  // tell jQuery not to set contentType
        success: function (result) {
            if (!result.includes("PLWEB.ERROR")) {
                $("#successMsg").show();
                $("#errorMessage").hide();
                $("#waitMsg").hide();
                console.log(result);
            }
            else {
                console.log(result);
                $("#errorMessage").show();
                $("#successMsg").hide();
                $("#waitMsg").hide();
            }

            $("#loader").hide();
            $("#upload").show();
        },
        error: function (jqXHR) {
        },
        complete: function (jqXHR, status) {
            //alert(status);
        }
    });


}