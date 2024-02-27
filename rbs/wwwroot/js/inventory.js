var InventoriesList = [];
var RowNumber = 0;
var LastRowNumber = -1;
var manufacurerData;
var inventoryData;
var IsNewRow = true;

function setAgentType() {
    var agent = JSON.parse(getCookie("agent"));
    if (agent !== null)
        agentType = agent.AgentTypeId;
}
$(document).ready(function () {
    ChcekCookie();
    $("#inventoryForm").hide();
    $("#loader").show();
    setAgentType();
    LoadTechs();
    LoadManufacurers();
    LoadProducts(0);
    $('#txtProductNo_1').focusout(function () {
        
        if (IsNewRow === true) {
            IsNewRow = false;
            RowNumber = RowNumber === null ? 0 : RowNumber + 1;
        }
        var ctrlId = Number(this.id.split("_")[1]);
        GetProduct(this.value, ctrlId);
        $("#btnSaveList").show();
    });

    $("input[name='optradio']").click(function () {
        var radioValue = $("input[name='optradio']:checked").val();
        if (radioValue) {
            $("#agents").show();
        }
    });

    $("#DDLTechs").change(function () {

        if ($("#div_1").length) {
            $("#div_1").show();
        }
        else {
            //$("#btnSaveList").hide();                    
            var controls = "<div class='form-inline' id='div_1'><input type ='text' id = 'txtProductNo_1' placeholder = 'Product #' class= 'form-control' style='margin-right:5px'/></div >";
            $("#divDetail").html("");
            document.getElementById('divDetail').innerHTML = controls;
            $("#div_1").css("padding-top", "10px");
            $('#txtProductNo_1').focusout(function () {
                RowNumber = 1;
                IsNewRow = true;
                var ctrlId = Number(this.id.split("_")[1]);
                GetProduct(this.value, ctrlId);

                $("#btnSaveList").show();
            });

        }
    });



    $("#btnSaveList").click(function () {
        SaveList();
    });
});

function CreateProductNumberBox() {
    LastRowNumber = RowNumber === null ? 1 : RowNumber;
    RowNumber = RowNumber === null ? 0 : RowNumber + 1;
    var nextindex = RowNumber;
    //var isDivExists = $("#div_" + LastRowNumber);

    // if (isDivExists === null) {
    $("#div_" + LastRowNumber + ":last").after("<div class='form-inline' id='div_" + nextindex + "'></div>");

    // }

    var productNumber = $('<input />', {
        type: 'text',
        class: 'form-control',
        id: "txtProductNo_" + nextindex,
        placeholder: "Product #",
        focusout: function () {
            var ctrlId = Number(this.id.split("_")[1]);
            IsNewRow = true;
            GetProduct(this.value, ctrlId);
        },
        focusin: function () {
            var ctrlId = Number(this.id.split("_")[1]);
            if (ctrlId > 1) {
                var id = ctrlId - 1;
                $("#txtProductNo_" + id).prop("disabled", true);
                $("#txtSN_" + id).prop("disabled", true);
                $("#ddl_" + id).prop("disabled", true);
                $("#ddlProducts_" + id).prop('disabled', true);
            }
        }

    });

    productNumber.appendTo($("#div_" + nextindex));
    $("#div_" + nextindex).css("padding-top", "10px");
}

function CreateRow(RowNo) {
    var nextindex = RowNo;

    var isDivExists = $("#div_" + LastRowNumber);

    if (isDivExists === null) {
        $("#div_" + LastRowNumber + ":last").after("<div class='form-inline' id='div_" + nextindex + "'></div>");
    }


    var serialNumberBox = $('<input />', {
        id: "txtSN_" + nextindex,
        class: "form-control",
        type: "text",
        placeholder: "Serial #"
    });

    var s = $('<select />', {
        id: "ddl_" + nextindex,
        class: "form-control",
        change: function () {
            var Id = Number(this.id.split("_")[1]);
            $("#ddlProducts_" + Id).prop('disabled', false);
            if (this.value === "-1") {
                var DDL = document.getElementById("ddlProducts_" + Id);
                $("#ddlProducts_" + Id + " option").remove();
                DDL.innerHTML = DDL.innerHTML +
                    '<option value="-1">--Select Product---</option>';
                for (var j = 0; j < inventoryData.length; j++) {
                    DDL.innerHTML = DDL.innerHTML +
                        '<option value="' + inventoryData[j].productId + '">' + inventoryData[j].productName + '</option>';
                }
            }
            else {
                LoadProducts(this.value, Id);
            }
        }
    });
    var removebutton = $('<input />', {
        value: 'Remove',
        type: 'button',
        class: 'btn btn-primary',
        id: "btn_" + nextindex,
        disabled: true,
        click: function () {
            var deleteindex = this.id.split("_")[1];
            $("#div_" + deleteindex).remove();

        }
    });

    var comboProducts = $('<select />', {
        id: "ddlProducts_" + nextindex,
        class: "form-control",
        change: function () {            
            if (this.value !== -1) {
                var divId = Number(this.id.split("_")[1]) + 1;
                var CheckRow = document.getElementById("div_" + divId);
                if (CheckRow === null) {
                    $("#btn_" + nextindex).prop('disabled', false);
                    IsNewRow = true;
                    CreateProductNumberBox();
                }
            }
            else {
                $("#btn_" + nextindex).prop('disabled', true);
            }
        }
    });

    $('<option />', { value: "-1", text: "All" }).appendTo(s);
    for (var i = 0; i < manufacurerData.length; i++) {
        $('<option />', { value: manufacurerData[i].manufacturerId, text: manufacurerData[i].manufacturer }).appendTo(s);
    }

    $('<option />', { value: "-1", text: "Select Product" }).appendTo(comboProducts);
    for (var ii = 0; ii < inventoryData.length; ii++) {
        $('<option />', { value: inventoryData[ii].productId, text: inventoryData[ii].productName }).appendTo(comboProducts);
    }

    serialNumberBox.appendTo($("#div_" + nextindex));
    $("#txtSN_" + nextindex).focus();
    if (nextindex !== 1) {
        $(serialNumberBox).css("margin-left", "5px");
    }
    s.appendTo($("#div_" + nextindex));
    $(s).css("margin-left", "5px");
    $(s).css("width", "200px");
    comboProducts.appendTo($("#div_" + nextindex));
    $(comboProducts).css("margin-left", "5px");
    $(comboProducts).css("width", "200px");
    removebutton.appendTo($("#div_" + nextindex));
    $(removebutton).css("margin-left", "5px");

}

function LoadTechs() {
    $("#inventoryForm").hide();
    $("#loader").show();
    $.ajax({
        url: "/api/Agent/50", //tech is agentType50
        type: "GET",
        dataType: "json",
        success: function (data, textStatus, xhr) {
            var DDLTechs = document.getElementById("DDLTechs");
            $("#DDLTechs option").remove();
            DDLTechs.innerHTML = DDLTechs.innerHTML +
                '<option value="-1">--Who--</option>';
            for (var i = 0; i < data.length; i++) {
                DDLTechs.innerHTML = DDLTechs.innerHTML +
                    '<option value="' + data[i].agentId + '">' + data[i].firstName + ' ' + data[i].lastName + '</option>';
            }
            $("#inventoryForm").show();
            $("#loader").hide();
        },
        error: function (xhr, textStatus, errorThrown) {
            console.log("Error in Operation");
        }
    });
}

function LoadManufacurers() {
    $.ajax({
        url: "/api/Inventory",
        type: "GET",
        dataType: "json",
        success: function (data, textStatus, xhr) {
            manufacurerData = data;
        },
        error: function (xhr, textStatus, errorThrown) {
            console.log("Error in Operation");
        }
    });
}

function LoadProducts(manufacturer, ctrlId) {
    var inventoryObject = new Object();
    inventoryObject.ManufacturerId = manufacturer;

    $.ajax({
        url: "/api/Inventory",
        type: "Post",
        dataType: "json",
        contentType: "application/json",
        data: JSON.stringify(inventoryObject),
        success: function (data, textStatus, xhr) {
            if (manufacturer === 0) {
                inventoryData = data;
            } else {
                var DDL = document.getElementById("ddlProducts_" + ctrlId);
                $("#ddlProducts_" + ctrlId + " option").remove();
                DDL.innerHTML = DDL.innerHTML +
                    '<option value="-1">--Select Product---</option>';
                for (var i = 0; i < data.length; i++) {
                    DDL.innerHTML = DDL.innerHTML +
                        '<option value="' + data[i].productId + '">' + data[i].productName + '</option>';
                }
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            console.log("Error in Operation");
        }
    });
}

function GetProduct(ProductNumber, ctrlId) {

    $("#ddl_" + ctrlId).prop('disabled', false);
    $("#ddlProducts_" + ctrlId).prop('disabled', false);

    $("#ddl_" + ctrlId).val(-1);
    $("#ddlProducts_" + ctrlId).val(-1);

    $.ajax({
        url: "/api/Inventory/" + ProductNumber,
        type: "GET",
        dataType: "json",
        contentType: "application/json",
        success: function (data, textStatus, xhr) {
            if ($("#txtProductNo_" + ctrlId).val() !== "") {
                var txtSNExists = document.getElementById("txtSN_" + ctrlId);
                if (txtSNExists === null) {
                    CreateRow(RowNumber);
                }
            }
            if (typeof (data) !== "undefined") {

                if (data !== null) {
                    if (data.manufacturerId !== null) {
                        if ($("#txtProductNo_" + ctrlId).val() !== "") {
                            $("#ddl_" + ctrlId).val(data.manufacturerId);
                            $("#ddlProducts_" + ctrlId).val(data.productId);
                            $("#ddl_" + ctrlId).prop('disabled', true);
                            $("#ddlProducts_" + ctrlId).prop('disabled', true);
                            $("#txtProductNo_" + ctrlId).prop('disabled', true);
                        }
                        if ($("txtProductNo_" + ctrlId).val() !== "" && $("#ddlProducts_" + ctrlId).val() > 0) {
                            CreateProductNumberBox();
                        }
                    }
                }
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            console.log("Error in Operation");
        }

    });
}

function SaveList() {
    $("#msg").html("");
    $("#errorMessage").hide();
    InventoriesList = [];
    var inOrOut = $("input[name='optradio']:checked").val();
    if (typeof (inOrOut) === "undefined") {
        $("#msg").html("Select IN OR OUT First.");
        $("#errorMessage").show();
        return false;
    }

    if ($("#DDLTechs").val() === "-1") {
        $("#msg").html("Select agent First.");
        $("#errorMessage").show();
        return false;
    }

    for (var i = 0; i < RowNumber; i++) {
        var CheckRow = document.getElementById("div_" + i);
        if (CheckRow !== null) {
            var obj = new Object();
            obj.AgentId = $("#DDLTechs").val();
            obj.InOrOut = inOrOut;
            obj.ProductNumber = $("#txtProductNo_" + i).val();
            obj.SerialNumber = $("#txtSN_" + i).val();
            obj.ManuText = $("#ddl_" + i + " option:selected").text();
            obj.ProductText = $("#ddlProducts_" + i + " option:selected").text();
            InventoriesList.push(obj);
        }
    }
    SaveToServer();
}

function SaveToServer() {
    $.ajax({
        url: "/api/Inventory",
        type: "PUT",
        dataType: "json",
        contentType: "application/json",
        data: JSON.stringify(InventoriesList),
        success: function (data, textStatus, xhr) {
            if (data === true) {
                window.location.replace("/thanks.html");
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            console.log(xhr);
            console.log(textStatus);
            console.log(errorThrown);
            console.log("Error in Operation");
        }
    });
}