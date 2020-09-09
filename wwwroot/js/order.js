"use strict";

$(() => {
    if ($('#fingers10').length !== 0) {

        var table = $('#fingers10').DataTable({
            //"scrollX": true,
            //"scrollY": "" + (window.outerHeight - 375) + "px",
            language: {
                processing: "Loading Data...",
                zeroRecords: "No matching records found"
            },
            processing: true,
            serverSide: true,
            orderCellsTop: true,
            searching: true,
           // autoWidth: true,
            deferRender: true,
            "pageLength": 10,
            "lengthChange": true,
            //dom: '<"row"<"col-sm-12 col-md-6"B><"col-sm-12 col-md-6 text-right"l>><"row"<"col-sm-12"tr>><"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
            dom: '<"html5buttons"B>lTfgitp',
            buttons: [
                { extend: 'copy' },
                {
                    extend: 'csv', exportOptions: {
                        columns: [0, 1, 2, 3, 4, 5]
                    }
                },
                {
                    extend: 'excel', title: 'CareGiverList'
                },

                {
                    extend: 'pdf', title: 'CareGiverList',
                    customize: function (doc) {
                        doc.pageMargins = [10, 10, 10, 10];
                        doc.defaultStyle.fontSize = 7;
                        doc.styles.tableHeader.fontSize = 7;
                        doc.styles.title.fontSize = 9;
                        // Remove spaces around page title
                        doc.content[0].text = "Standard Precautions HIPPA \n Emergency Preparedness Home Safety";
                        doc.content[1].table.widths =
                            Array(doc.content[1].table.body[0].length + 1).join('*').split('');
                    }
                },

                {
                    extend: 'print',
                    customize: function (win) {
                        $(win.document.body).addClass('white-bg');
                        $(win.document.body).css('font-size', '10px');

                        $(win.document.body).find('table')
                            .addClass('compact')
                            .css('font-size', 'inherit');
                    }
                }
            ],
            ajax: {
                type: "POST",
                url: '/CareGiver/LoadTable/',
                contentType: "application/json; charset=utf-8",
                async: true,
                data: function (data) {
                    let additionalValues = [];
                    additionalValues[0] = $("#startDate").val();
                    additionalValues[1] = $("#endDate").val();
                    data.AdditionalValues = additionalValues;
                    return JSON.stringify(data);
                },
                beforeSend: function () {
                    // Here, manually add the loading message.
                    $('#fingers10 > tbody').html(
                        '<tr class="odd">' +
                        '<td valign="top" colspan="10" class="dataTables_empty">Loading&hellip;</td>' +
                        '</tr>'
                    );
                },
            },
            columns: [
                {
                    title: "First Name",
                    data: "FirstName",
                    name: "co"

                },
                {
                    title: "Middle Name",
                    data: "MiddleName",
                    name: "co"
                },
                {

                    title: "Last Name",
                    data: "LastName",
                    name: "co"
                },
                {
                    title: "Email",
                    data: "Email",
                    name: "co"
                },
                {
                    title: "Employee No",
                    data: "EmployeeNo",
                    name: "co"
                },
                {
                    title: "Hr Group",
                    data: "HrGroupName",
                    name: "co"
                },
                {
                    title: "Test Status",
                    data: "PassedTest",
                    searchable: false,
                    render: function (data, type, row) {
                        return '<a href="ViewTest/' + row.EmployeeId + '">' + row.PassedTest + '  of ' + row.Totaltest + ' passed</a>';
                    }
                },
                {
                    title: "Exam Date",
                    render: function (data, type, row) {
                        if (row.ExamDate)
                            return window.moment(row.ExamDate).format("MM/DD/YYYY");
                        else
                            return null;
                    },
                    data: "ExamDate",
                    name: "eq"
                },
                {
                    title: "Action",
                    data: "UserId",
                    searchable: false,
                    render: function (data, type, row) {
                        if (row.UserId === 0) {
                            if (row.ValidEmail) {
                                return '<a href="#" onclick="sendEmail(\'' + row.Email + '\')">Send Registration Link</a>';
                            }
                            else {
                                return '<span class="label label-danger">Invalid Email</span>';
                            }
                        }
                        else {
                            return '<a href="#" onclick="sendEmailLogin(\'' + row.Email + '\')">Send Login Link</a>';
                        }
                    }
                },
                {
                    title: "Video (Time)",
                    data: "VideoDuration",
                    render: function (data, type, row) {
                        return '<span style="display:none" class="cUserId">' + row.UserId + '</span>'+ secondsToHms(data);
                    },
                    name: "co"
                }


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

        //$("input").on("change", function (e) {
        //    table.draw();
        //});

        $(".search").on("click", function (e) {

            var startDate = $("#startDate").val();
            var endDate = $("#endDate").val();
            if ((!startDate) || (!endDate)) {
                swal("Please select both dates", "", "error");
                return;
            }
            console.log(startDate);
            console.log(endDate);
            if (startDate > endDate) {
                swal("From date should be greater or equal to current date", "", "error");
                return;
            }
            table.draw();
            $("#btnPrint").show();
        });




    }
});
