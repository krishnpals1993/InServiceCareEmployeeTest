"use strict";

$(() => {
    if ($('#fingers10').length !== 0) {

        var table = $('#fingers10').DataTable({
            "scrollY": "" + (window.outerHeight - 375) + "px",
            "scrollX": "" + (window.outerWidth - 300) + "px",
            "scrollCollapse": true,
            language: {
                processing: "Loading Data...",
                zeroRecords: "No matching records found"
            },
            processing: true,
            serverSide: true,
            orderCellsTop: true,
            searching: true,
            autoWidth: true,
            deferRender: true,
            "pageLength": 10,
            "lengthChange": false,
            dom: '<"row"<"col-sm-12 col-md-6"B><"col-sm-12 col-md-6 text-right"l>><"row"<"col-sm-12"tr>><"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
            buttons: [

            ],
            ajax: {
                type: "POST",
                url: '/Invoice/LoadTable/',
                contentType: "application/json; charset=utf-8",
                async: true,
                data: function (data) {
                    let additionalValues = [];
                    additionalValues[0] = $("#InvoiceNum").val();
                    additionalValues[1] = $("#OrderNum").val() ? $("#OrderNum").val() : '';
                    additionalValues[2] = $("#BillName").val() ? $("#BillName").val() : '';
                    data.AdditionalValues = additionalValues;

                    return JSON.stringify(data);
                }
            },
            columns: [
                {
                    orderable: false,
                    width: 100,
                    data: "Action",
                    render: function (data, type, row) {

                        return `<div>
                                    <a   href="#" class="btnEdit" data-key="${row.InvoiceNum}">Detail</button>
                                 </div>`;
                    }
                },
                {
                    data: "ShipDate",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "CancelDate",
                    searchable: false,
                    name: "eq"
                },
                {
                    data: "OrderNum",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "CustNum",
                    name: "co"
                },
                {
                    data: "InvoiceNum",
                    name: "co"
                },
                {
                    data: "OrderDate",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "BillName",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "BillAddress1",
                    searchable: false,
                    name: "co"

                },
                {
                    data: "BillAddress2",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "BillAddress3",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "ShipName",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "ShipAddress1",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "ShipAddress2",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "ShipAddress3",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "InvoiceDate",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "ReleaseDate",
                    searchable: false,
                    name: "eq"
                },
                {
                    data: "Freight",
                    searchable: false,
                    name: "co"
                },

                {
                    data: "SalesManNum",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "SalesManNum2",
                    searchable: false,
                    name: "co"
                },

                {
                    data: "TermId",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "CarrierCode",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "D2",
                    searchable: false,
                    name: "eq"
                },
                {
                    data: "PoNum",
                    searchable: false,
                    name: "co"
                },


                {
                    data: "EnterDate",
                    searchable: false,
                    name: "eq"
                },
                {
                    data: "Terminal",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "CustNote",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Clerk",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "NetTotal",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Commision",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "D4",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "D6",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "StoreNum",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Dept",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "OrderType",
                    searchable: false,
                    name: "eq"
                },
                {
                    data: "BolNum",
                    name: "co",
                    searchable: false
                },
                {
                    data: "BolDate",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Tracking1",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Tracking2",
                    searchable: false,
                    name: "co"
                },


                {
                    data: "IsCreditMemo",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "IsCreditHold",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "IsFreightCollect",
                    searchable: false,
                    name: "co"
                },

            ]
        });

        table.columns().every(function (index) {
            $('#fingers10 thead tr:last th:eq(' + index + ') input')
                .on('keyup',
                    function (e) {
                        if (e.keyCode === 13) {
                            table.column($(this).parent().index() + ':visible').search(this.value).draw();
                        }
                    });
        });

        $("input").on("change", function (e) {
            table.draw();
        });

        $(document)
            .off('click', '.btnEdit')
            .on('click', '.btnEdit', function () {
                const id = $(this).attr('data-key');
                window.location.href = window.baseUrl + "/Invoice/Detail/" + id;

            });


    }
});
