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
            searching:true,
            autoWidth: true,
            deferRender: true,
            "pageLength": 10,
            "lengthChange": false,
            dom: '<"row"<"col-sm-12 col-md-6"B><"col-sm-12 col-md-6 text-right"l>><"row"<"col-sm-12"tr>><"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
            buttons: [
                
            ],
            ajax: {
                type: "POST",
                url: '/Order/LoadTable/',
                contentType: "application/json; charset=utf-8",
                async: true,
                data: function (data) {
                    let additionalValues = [];
                    additionalValues[0] = $("#ordernum").val();
                    additionalValues[1] = $("#Name").val();
                    additionalValues[2] = $("#Shipdate").val();
                    data.AdditionalValues = additionalValues;

                    return JSON.stringify(data);
                }
            },
            columns: [
                {
                    orderable: false,
                  
                    data: "Action",
                    render: function (data, type, row) {

                        return `<div>
                                    <a   href="#" class="btnEdit" data-key="${row.ordernum}">Detail</button>
                                 </div>`;
                    }
                },
                {
                    orderable: false,
                    data: "Notes",
                    render: function (data, type, row) {
                        if (row.NoteCount > 0) {
                            return `<a data-toggle="modal" data-target="#notes"
                                               onclick="openNote(`+ row.ordernum + `,'` + row.Orderdate+`')" href="#">
                                                <i class="fa fa-sticky-note-o"></i>
                                            </a>`;
                        }
                        else {
                            return ``;
                        }


                    }

                },
                {
                    data: "Shipdate",
                    render: function (data, type, row) {
                        if (data) {
                            console.log(data);
                            var date1 = new window.moment(window.moment().format("MM/DD/YYYY"));
                            var date2 = new window.moment(data);
                            const diffTime = (date2 - date1);
                            const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24));

                            var formattedDate = window.moment(data).format("MM/DD/YYYY");
                            if (diffDays == 0) {
                                return '<span class="label label-warning">' + formattedDate + '</span>';
                            }
                            else if (diffDays >= 1 && diffDays < 3) {
                                return '<span class="label label-success">' + formattedDate + '</span>';
                            }
                            else if (diffDays >= 3 && diffDays < 7) {
                                return '<span class="label label-primary">' + formattedDate + '</span>';
                            }
                            else if (diffDays >= 7) {
                                return '<span class="label label-info">' + formattedDate + '</span>';
                            }
                            else if (diffDays < 0) {
                                return '<span class="label label-danger">' + formattedDate + '</span>';
                            }
                            return formattedDate;
                        }
                        else
                            return null;
                    },
                    searchable: false,
                    name: "eq"
                },
                {
                    data: "Canceldate",
                    render: function (data, type, row) {
                        if (data)
                            return window.moment(data).format("MM/DD/YYYY");
                        else
                            return null;
                    },
                    searchable: false,
                    name: "eq"
                },
                {
                    data: "ordernum",
                    name: "co"
                },
                {
                    data: "Custnum",
                    name: "co"
                },
                
                {
                    data: "Orderdate",
                    searchable: false,
                    render: function (data, type, row) {
                        if (data)
                            return window.moment(data).format("MM/DD/YYYY");
                        else
                            return null;
                    },
                    name: "eq"
                },
                {
                    data: "Name",
                    name: "co"
                },
                
                {
                    data: "Shipname",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Shipaddress1",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Shipaddress2",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Shipaddress3",
                    searchable: false,
                    name: "co"

                },
                {
                    data: "Address1",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Address2",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Address3",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Terms",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Via",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Backorder",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Tax",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Ponum",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Shippeddate",
                    searchable: false,
                    render: function (data, type, row) {
                        if (data)
                            return window.moment(data).format("MM/DD/YYYY");
                        else
                            return null;
                    },
                    name: "eq"
                },
                
                {
                    data: "Edidate",
                    render: function (data, type, row) {
                        if (data)
                            return window.moment(data).format("MM/DD/YYYY");
                        else
                            return null;
                    },
                    searchable: false,
                    name: "eq"
                },
                {
                    data: "Terminal",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Custnote",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "clerk",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Poammount",
                    searchable: false,
                    name: "eq"
                },
                {
                    data: "Commission",
                    searchable: false,
                    name: "eq"
                },
                {
                    data: "Status",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "D1",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "D2",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Creditmemo",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Storenum",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Dept",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Ordertype",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "WeborderId",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "IsOpenOrder",
                    searchable: false,
                    name: "co",
                    render: function (data, type, row) {
                        if (data) {
                            return `<span class="badge badge-success">Yes</span>`;
                        }
                        else {
                            return `<span class="badge badge-danger">No</span>`;
                        }
                     
                    }
                },
                {
                    data: "Credit",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "Freight",
                    searchable: false,
                    name: "eq"
                },
                {
                    data: "Slnum",
                    name: "co",
                    searchable: false
                },
                {
                    data: "Slnum2",
                    searchable: false,
                    name: "co"
                },
                {
                    data: "D0",
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
                window.location.href = window.baseUrl+"/Order/Detail/" + id;
                 
            });

      
    }
});
