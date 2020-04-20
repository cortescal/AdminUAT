
var icono_load = '<div class="text-center py-5"><i class="far fa-hourglass fa-spin fa-2x text-muted"></i></div>';
var spinner_sm = '<div class="spinner-border spinner-border-sm"></div>';
var load = '<div class="py-5 text-center text-muted"><p><i class="far fa-hourglass fa-spin fa-2x text-muted"></i></p><p>Cargando...</p></div>';
var alerta = '<i class="fas fa-exclamation-triangle"></i> Alerta';

function detailsDenuncia(id, action) {
    $.ajax({
        type: "POST",
        url: action,
        data: { id },
        beforeSend: function () {
            $("#detalles").html(load);
        },
        success: function (response) {           
           $("#detalles").html("");
           $("#detalles").append(response);
        },
        error: function () {
            $("#detalles").html(errorConexion);
        }
    });
}

function SendMailToken(id) {
    $.ajax({
        type: "GET",
        url: 'Denuncias/SendToken',
        data: { id },
        beforeSend: function () {
            //$("#detalles").html('<div class="text-center"><img src="/images/loading.gif" alt="" /></div>');
        },
        success: function (response) {
            
            if (response['resp'])
            {
                $("#msjToken").html('<div class="alert alert-success text-center">Exito al enviar el código a ' + response['email'] +'</div>');
            } else
            {
                $("#msjToken").html('<div class="alert alert-danger text-center">Error al enviar el código. Intenta de nuevo porfavor</div>');
            }
        }
    });
}

function AddComentario(action) {
    $.ajax({
        type: "POST",
        url: action,
        data: $("#formData").serialize(),
        beforeSend: function () {
            //$("#detalles").html('<div class="text-center"><img src="/images/loading.gif" alt="" /></div>');
        },
        success: function (response) {
            if (response['error']) {
                $("#msjToken").html('<div class="alert alert-danger text-center">' + response['msj'] + '</div>');
            } else {
                $("#btnMail").html('');
                $("#msjToken").html('<div class="alert alert-success text-center">' + response['msj'] + '</div>');
                $("#bodyForm").html('<div class="row"><div class="col-lg-6 col-md-12 col-sm-12"><h6 class="card-title">Respuesta</h6><p class="card-text text-monospace text-muted">' + response['solucion'] + '</p></div><div class="col-lg-6 col-md-12 col-sm-12"><h6 class="card-title">Fecha de solución</h6><p class="card-text text-monospace text-muted">' + response['fecha'] + '</p></div></div><div class="row"><div class="col-12"><h6 class="card-title">Nota</h6><p class="card-text text-monospace text-muted">' + response['nota'] + '</p></div></div>');
            }
        }
    });
}

function foliadora(num) {
    var aux = num.toString();
    while (aux.length < 5) {
        aux = "0" + aux;
    }
    return aux;
}


//************************* MDB ******************************
var errorConexion = '<div class="py-5 text-center text-muted"><p><i class="fas fa-wifi fa-2x"></i></p><p>!Error de conexión!. Intente de nuevo porfavor</p></div>';

function getVistaCitaMP(id) {
    $.ajax({
        type: "GET",
        url: '../Agenda/Index?id=' + id,
        beforeSend: function () {
            $("#vista_mp").html(icono_load + '<p class="text-center">Se esta generando la vista. Espere porfavor...<p>');
        },
        success: function (respuesta) {
            $('#vista_mp').html(respuesta);
        },
        error: function () {
            $('#vista_mp').html(errorConexion);
        }
    });
}

function mostrarHorarioMP() {
    $('#btn_form').click();
}

function miHorario(mpId, id_selector, ruta) {
    $('#modal_title').html('Mi Horario');
    $.ajax({
        type: 'get',
        url: ruta + "Agenda/MiHorario",
        data: { mpId },
        beforeSend: function () {
            $('#' + id_selector).html(load);
        },
        success: function (respuesta) {
            $('#' + id_selector).html(respuesta);
            $('select').selectpicker();
        },
        error: function () {
            $('#' + id_selector).html(errorConexion);
        }
    });
}

function viewAddHorario(mpId) {
    $('#modalTopLeft_title').html('Agregar horario');
    $.ajax({
        type: 'get',
        url: 'HoraDias/Create',
        data: { mpId },
        beforeSend: function () {
            $('#modalTopLeft_body').html(load);
        },
        success: function (respuesta) {
            $('#modalTopLeft_body').html(respuesta);
        },
        error: function () {
            $('#modalTopLeft_body').html(errorConexion);
        }
    });
}

function addHorario(mpId) {

    var dia = $('#dia').val();
    var horas = $('#horas').val();

    $('#alerta_dia').html('');
    $('#alerta_horas').html('');

    if (dia === "") {     
        $('#alerta_dia').html('*Selecciona un día');
    }

    if (horas.length === 0) {
        $('#alerta_horas').html('*Selecciona una o más horas');
    }

    if (horas.length > 0 && dia != "") {
        $.ajax({
            type: 'post',
            url: 'HoraDias/Create',
            data: { mpId, dia, horas },
            beforeSend: function () {
                $('#modalTopLeft_body').html(load);
            },
            success: function (respuesta) {
                $('#modalTopLeftPartial').modal('hide');
                window.location.replace(respuesta['ruta'] + "HoraDias?mpId=" + mpId);
            },
            error: function () {
                $('#modalTopLeft_body').html(errorConexion);
            }
        });
    }
}

function fecha_actual() {
    //var options = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };
    var options = { weekday: 'long', year: 'numeric', month: '2-digit', day: '2-digit' };
    $('#fecha').html(new Date().toLocaleDateString("es-ES", options));
}

function reloj() {
    $('#hora').html(new Date().toLocaleTimeString('en-US'));
    setTimeout('reloj()', '1000');
}

function getTokenAgenda(denunciaId, urlBase) {
    $.ajax({
        type: 'get',
        url: urlBase + 'Agenda/GetCodigo',
        data: { denunciaId },
        beforeSend: function () {
            $('#' + denunciaId).html(spinner_sm);
            $('#' + denunciaId).addClass('disabled');
        },
        success: function (respuesta) {
            $('#key' + denunciaId).html(respuesta['token']);
        },
        error: function () {
            $('#' + denunciaId).html('<i class="fas fa-key"></i>');
            $('#' + denunciaId).removeClass('disabled');
        }
    });
}

function addAsistencia(nomRadio, citaId, urlBase) {
    var asistencia = $('input:radio[name=' + nomRadio + ']:checked').val();
    if (asistencia === '0' || asistencia === '1') {
        $.ajax({
            type: 'get',
            url: urlBase + 'Agenda/AddAsistencia',
            data: { citaId, asistencia },
            beforeSend: function () {

            },
            success: function (respuesta) {
                $('#recarga_body_citas').html(respuesta);
            },
            error: function () {

            }
        });
    }
}

//++++++++++++++ Soporte ++++++++++++++++++++++
function viewSoporteCreate(ruta) {
    $('#modalTitleBottom').html('Mi solicitud');
    $('#modalFooterBottom').html('');

    $.ajax({
        type: 'get',
        url: ruta + '/AdminSoportes/Create',
        beforeSend: function () {
            $('#modalBodyBottom').html(load);
        },
        success: function (respuesta) {
            $('#modalBodyBottom').html(respuesta);
            $('select').selectpicker();
        },
        error: function () {
            $('#modalBodyBottom').html(errorConexion);
        }
    });
}

function crearSoporte(ruta) {
    var solicitud = $('#solicitud').val();
    var tipoSoporteId = $('#tipoSoporteId').val();
    $('#errorSolicitud').html('');
    $('#errorTipoSoporteId').html('');

    if (solicitud === "") {
        $('#errorSolicitud').html('* campo vacio');
    }

    if (tipoSoporteId === '0') {
        $('#errorTipoSoporteId').html('* selecciona una opción');
    }

    if (solicitud !== "" && tipoSoporteId !== '0') {
        var orden = $('#formSolicitudSoporte').serialize();
        $.ajax({
            type: 'post',
            url: ruta + '/AdminSoportes/Create',
            data: orden,
            beforeSend: function () {
                $('#modalBodyBottom').html(load);
            },
            success: function (respuesta) {
                $('#fullHeightModalRight').modal('hide');
                $('#adminSoportesIndexBody').html(respuesta);
            },
            error: function () {
                $('#modalBodyBottom').html(errorConexion);
            }
        });
    }
}

function notificaciones(ruta, soporteId) {
       
    $.ajax({
        type: 'get',
        url: ruta + '/AdminSoportes/Notificaciones',
        data: { soporteId },
        beforeSend: function () {
            blockPage();
        },
        success: function (respuesta) {
            refreshMisSolicitudes(ruta);
            $('#fullHeightModalRight').modal('show');
            $('#modalTitleBottom').html('<div class="text-monospace">Folio' + ' ' + foliadora(soporteId) + '</div>');
            $('#modalFooterBottom').html('');
            $('#modalBodyBottom').html(respuesta);
            campoMsjSeguimiento(ruta, soporteId);
            $.unblockUI();
        },
        error: function () {
            $.unblockUI();
            $('#fullHeightModalRight').modal('show');
            $('#modalTitleBottom').html(alerta);
            $('#modalBodyBottom').html(errorConexion);
        }
    });
}

function campoMsjSeguimiento(ruta, soporteId){
    $.ajax({
        type: 'get',
        url: ruta + '/AdminSoportes/MsjSeguimiento',
        data: { soporteId },
        success: function (respuesta) {
            $('#modalFooterBottom').html(respuesta);
        },
        error: function () {
            $('#modalBodyBottom').html(errorConexion);
        }
    });
}

function refreshMisSolicitudes(ruta) {
    $.ajax({
        type: 'get',
        url: ruta + '/AdminSoportes/Index?viewPartial=true',
        success: function (respuesta) {
            $('#adminSoportesIndexBody').html(respuesta);
        }
    });
}

function addSeguimiento(ruta, soporteId) {
    var comentario = $('#msjSeguimiento').val();
    if (comentario === "") {
        $('#errorMsjSeguimiento').html('* Mensaje vacio');
    }
    else {
        $.ajax({
            type: 'post',
            url: ruta + '/AdminSoportes/AddSeguimiento',
            data: { soporteId, comentario },
            beforeSend: function () {
                $('#btnCampoMsj').addClass('disabled');
                $('#btnCampoMsj').html('<span class="spinner-border spinner-border-sm"></span> Enviando...');
            },
            success: function (respuesta) {
                $('#msjSeguimiento').val('');
                $('#modalBodyBottom').html(respuesta);
                $('#btnCampoMsj').removeClass('disabled');
                $('#btnCampoMsj').html('Enviar');
            },
            error: function () {
                $('#modalFooterBottom').html(errorConexion);
                $('#btnCampoMsj').removeClass('disabled');
                $('#btnCampoMsj').html('Enviar');
            }
        });
    }
}

function confirmaCerrarSoporte(ruta, id) {
    $('#modalClasicoTitle').html(alerta);

    $.ajax({
        type: 'get',
        url: ruta,
        data: { id },
        beforeSend: function () {
            $('#modalBodyBottom').html(load);
        },
        success: function (respuesta) {
            $('#modalClasicoBody').html(respuesta);
            $('#folio').html(foliadora(id));
        },
        error: function () {
            $('#modalClasicoBody').html(errorConexion);
        }
    });
}


//++++++++++++  Funciones para Controller usuarios  +++++++++++++++++
function cambiarUr(mpId) {
    var urId = $('#URId').val();
    $.ajax({
        type: 'get',
        url: 'Usuarios/ActualizarUR',
        data: { mpId, urId },
        beforeSend: function () {
            $('#msjUrOk').html(load);
        },
        success: function () {           
            $('#msjUrOk').html('<hr/><p><i class="far fa-thumbs-up"></i></p><p>Actualización de UR con exito</p>');
        },
        error: function () {
            $('#msjUrOk').html(errorConexion);
        }
    });
}


function blockPage() {
    $.blockUI({
        message: '<i class="far fa-hourglass fa-spin fa-2x"></i>',
        css: {
            border: 'none',
            padding: '15px',
            backgroundColor: '#000',
            '-webkit-border-radius': '10px',
            '-moz-border-radius': '10px',
            opacity: .5,
            color: '#fff'
        }
    });
}



//*************************** Modal de lareta de confirmacción *****************************
function alertaConfirmacion(ruta, id) {
    $('#modalClasicoTitle').html(alerta);

    $.ajax({
        type: 'get',
        url: ruta,
        data: { id },
        beforeSend: function () {
            $('#modalBodyBottom').html(load);
        },
        success: function (respuesta) {
            $('#modalClasicoBody').html(respuesta);
        },
        error: function () {
            $('#modalClasicoBody').html(errorConexion);
        }
    });
}