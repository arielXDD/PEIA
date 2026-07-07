document.addEventListener('DOMContentLoaded', () => {
  renderGuideShell();

  if (!PEIA.requireAuth()) return;
  PEIA.hydrateShell();
  PEIA.bindWarehouseSelector();

  const topicKey = document.body.dataset.guideTopic || 'intro';
  const topics = {
    intro: {
      label: 'Modulo base',
      title: 'Introduccion a PEIA',
      subtitle: 'Manual para iniciar sesion, elegir centro, entender permisos y moverse sin perderse por los modulos principales.',
      image: 'linear-gradient(90deg, rgba(29,78,216,.94), rgba(8,145,178,.78)), url("https://images.unsplash.com/photo-1551434678-e076c223a692?auto=format&fit=crop&w=1500&q=80")',
      summary: [
        'PEIA organiza la operacion por centros de trabajo. Antes de capturar o consultar informacion, el usuario debe confirmar que esta en el centro correcto y que su rol tiene permisos para la tarea que necesita realizar.',
        'Esta guia explica el recorrido basico para comenzar el turno: iniciar sesion, revisar identidad, seleccionar bodega, ubicar el modulo correcto, atender alertas y cerrar sesion de forma segura.'
      ],
      modules: ['Inicio', 'Inventario', 'Pedidos', 'Reportes', 'Roles', 'Camaras', 'Prediccion', 'Notificaciones', 'Usuarios', 'Configuracion', 'Bitacora', 'Guia'],
      procedures: [
        {
          title: 'Entrar al sistema correctamente',
          steps: [
            'Abre PEIA desde el navegador autorizado por la empresa.',
            'Escribe usuario y contrasena exactamente como fueron asignados.',
            'Si el acceso falla, revisa mayusculas, espacios al final y estado de la cuenta.',
            'No uses la cuenta de otro trabajador para evitar errores de auditoria.'
          ]
        },
        {
          title: 'Confirmar usuario, rol y centro activo',
          steps: [
            'Revisa el nombre de usuario en la esquina superior derecha.',
            'Confirma que el centro activo corresponde a tu bodega de trabajo.',
            'Si trabajas en mas de un centro, cambia el selector antes de capturar datos.',
            'Recuerda que inventario, pedidos, reportes y alertas usan ese centro como contexto.'
          ]
        },
        {
          title: 'Ubicar una herramienta en el menu lateral',
          steps: [
            'Usa Inicio para ver el resumen operativo del dia.',
            'Usa Inventario para productos, stock y movimientos.',
            'Usa Pedidos para registrar, asignar y consultar entregas.',
            'Usa Reportes para revisar KPIs, movimientos y descargar informacion.',
            'Usa Notificaciones para alertas de stock, pedidos o incidencias.',
            'Usa Configuracion, Roles y Usuarios solo si tu perfil administrativo lo permite.'
          ]
        }
      ],
      articles: [
        {
          title: 'Inicio: lectura del tablero principal',
          body: 'El tablero de Inicio resume el estado del centro activo. Los indicadores muestran productos en stock, pedidos pendientes, alertas, entradas, salidas e incidencias. Antes de entrar a un modulo especifico, revisa si hay alertas criticas o movimientos anormales; esto ayuda a priorizar el trabajo del turno.'
        },
        {
          title: 'Permisos: por que algunos menus no aparecen',
          body: 'PEIA muestra opciones segun el rol del usuario. Un operador puede ver inventario y pedidos, mientras que un administrador puede ver usuarios, roles y configuracion. Si una opcion no aparece, no es falla visual necesariamente: puede ser una restriccion de permisos.'
        },
        {
          title: 'Bitacora: rastro de acciones importantes',
          body: 'La bitacora sirve para consultar acciones realizadas en el sistema: cambios de inventario, acciones sobre pedidos y eventos relevantes. Cuando haya dudas sobre quien hizo un cambio, revisa fecha, usuario, modulo y descripcion del evento.'
        },
        {
          title: 'Pedidos: seguimiento logistico',
          body: 'Pedidos concentra la creacion, asignacion y seguimiento de solicitudes. El usuario debe revisar estado, cliente, fecha estimada, responsable y avance. Si un pedido cambia de estado, debe existir una razon operativa clara para que reportes y SLA mantengan coherencia.'
        },
        {
          title: 'Camaras: incidencias visuales',
          body: 'Camaras permite revisar puntos de monitoreo asociados a la operacion. Se usa para detectar incidencias, validar eventos y apoyar investigaciones. Cuando una camara marca problema, registra la observacion y relaciona el evento con inventario, pedido o bitacora si corresponde.'
        },
        {
          title: 'Prediccion: anticipar demanda',
          body: 'Prediccion ayuda a estimar comportamiento futuro con base en historicos. No reemplaza la revision operativa: sirve como senal para anticipar compras, preparar stock o detectar productos con demanda cambiante.'
        },
        {
          title: 'Notificaciones: bandeja de prioridades',
          body: 'Notificaciones muestra avisos que requieren atencion: stock bajo, pedidos pendientes, reglas disparadas o incidencias. Al iniciar turno, revisa primero las criticas, despues las medias y finalmente las informativas.'
        },
        {
          title: 'Usuarios y Roles: control de acceso',
          body: 'Usuarios administra cuentas; Roles define permisos. Los cambios deben hacerse con cuidado porque afectan que puede ver o modificar cada trabajador. Si alguien no puede usar una funcion, revisa primero su rol antes de asumir falla del sistema.'
        },
        {
          title: 'Configuracion: parametros del sistema',
          body: 'Configuracion concentra datos de empresa, preferencias, seguridad, notificaciones e integraciones. Debe usarse solo por perfiles autorizados, porque un cambio incorrecto puede afectar reportes, formatos, avisos o integraciones.'
        }
      ],
      fields: [
        ['Centro activo', 'Define la bodega sobre la que se consultan y capturan datos.'],
        ['Usuario', 'Identifica a la persona responsable de cada accion.'],
        ['Rol', 'Determina permisos y modulos visibles.'],
        ['Notificaciones', 'Avisos que deben revisarse al inicio del turno.']
      ],
      checks: ['Centro correcto seleccionado', 'Usuario propio activo', 'Alertas revisadas', 'Modulo correcto identificado', 'Sesion cerrada al terminar'],
      mistakes: ['Capturar informacion en una bodega equivocada', 'Compartir contrasenas', 'Ignorar alertas del tablero', 'Trabajar con una sesion abierta por otra persona']
    },
    inventario: {
      label: 'Operacion diaria',
      title: 'Gestion de Inventario',
      subtitle: 'Manual operativo para productos, categorias, stock, entradas, salidas, ajustes, busquedas y trazabilidad por centro.',
      image: 'linear-gradient(90deg, rgba(29,78,216,.92), rgba(15,23,42,.58)), url("https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?auto=format&fit=crop&w=1500&q=80")',
      summary: [
        'Inventario es el modulo donde se administra lo que existe fisicamente en cada bodega. La informacion debe mantenerse actualizada porque reportes, pedidos, alertas y decisiones de compra dependen de estos datos.',
        'El objetivo es que cualquier trabajador pueda consultar un producto, validar stock, registrar movimientos y entender el historial sin alterar informacion de otros centros.'
      ],
      modules: ['Productos', 'Categorias', 'Stock', 'Entradas', 'Salidas', 'Ajustes', 'Historial', 'Filtros'],
      procedures: [
        {
          title: 'Buscar un producto antes de crearlo',
          steps: [
            'Entra a Inventario y confirma el centro activo.',
            'Busca por SKU, nombre o categoria en la barra de busqueda.',
            'Aplica filtro de categoria o estado de stock si hay muchos resultados.',
            'Si el producto existe, edita o registra movimiento; no crees duplicados.'
          ]
        },
        {
          title: 'Dar de alta un producto',
          steps: [
            'Haz clic en Nuevo producto.',
            'Captura SKU unico, nombre claro, categoria, unidad de medida y ubicacion.',
            'Define stock minimo para que las alertas funcionen.',
            'Agrega stock inicial solo si tienes evidencia fisica del conteo.',
            'Guarda y confirma que el producto aparezca en la tabla.'
          ]
        },
        {
          title: 'Registrar entrada de mercancia',
          steps: [
            'Localiza el producto correcto y abre la accion de movimiento.',
            'Selecciona tipo Entrada.',
            'Captura cantidad recibida, referencia, motivo y ubicacion.',
            'Verifica que el stock final coincida con la recepcion fisica.',
            'Si hay diferencia, registra observacion para auditoria.'
          ]
        },
        {
          title: 'Registrar salida o consumo',
          steps: [
            'Selecciona el producto desde la tabla de inventario.',
            'Elige tipo Salida y captura la cantidad exacta.',
            'Indica motivo: venta, pedido, merma, transferencia o consumo interno.',
            'No permitas stock negativo. Si falta stock, revisa si una entrada no fue capturada.',
            'Guarda el movimiento y consulta el historial si hay dudas.'
          ]
        },
        {
          title: 'Hacer ajuste por conteo fisico',
          steps: [
            'Usa Ajuste solo despues de contar fisicamente.',
            'Compara el conteo contra el stock en sistema.',
            'Captura la diferencia con motivo claro.',
            'No uses ajuste para reemplazar una entrada o salida normal.',
            'Documenta responsable, fecha y evidencia si la empresa lo solicita.'
          ]
        }
      ],
      articles: [
        {
          title: 'Como leer el estado de stock',
          body: 'Stock normal significa que la cantidad disponible supera el minimo definido. Stock bajo indica que el producto requiere seguimiento o reposicion. Agotado significa que no debe prometerse para pedidos hasta registrar entrada o corregir el inventario.'
        },
        {
          title: 'Trazabilidad: seguir el rastro de un SKU',
          body: 'La trazabilidad se revisa desde el historial de movimientos. Cada movimiento debe explicar quien lo hizo, cuando, cantidad, tipo, centro y referencia. Si un producto cambia mucho, revisa entradas y salidas en orden cronologico para detectar diferencias.'
        },
        {
          title: 'Categorias y nombres consistentes',
          body: 'Las categorias permiten filtrar reportes y controlar familias de productos. Evita nombres ambiguos, abreviaturas locales o duplicados. Un buen nombre ayuda a que otros usuarios encuentren el producto sin depender de memoria.'
        }
      ],
      fields: [
        ['SKU', 'Codigo unico del producto. No debe repetirse.'],
        ['Categoria', 'Familia operativa para filtros y reportes.'],
        ['Stock minimo', 'Cantidad que dispara seguimiento o alerta de reposicion.'],
        ['Ubicacion', 'Lugar fisico donde se encuentra el producto.'],
        ['Referencia', 'Documento, pedido o motivo que justifica el movimiento.']
      ],
      checks: ['Centro correcto', 'SKU validado', 'Categoria asignada', 'Stock minimo definido', 'Movimiento con motivo', 'Historial revisado'],
      mistakes: ['Crear duplicados', 'Registrar salidas sin referencia', 'Usar ajuste para ocultar errores', 'Olvidar revisar el centro activo', 'Dejar productos sin stock minimo']
    },
    reportes: {
      label: 'Analisis operativo',
      title: 'Interpretacion de Reportes',
      subtitle: 'Manual para leer KPIs, filtrar informacion, detectar desviaciones y convertir datos en decisiones operativas.',
      image: 'linear-gradient(90deg, rgba(29,78,216,.94), rgba(124,58,237,.62)), url("https://images.unsplash.com/photo-1551288049-bebda4e38f71?auto=format&fit=crop&w=1500&q=80")',
      summary: [
        'Reportes convierte la actividad del sistema en indicadores. Sirve para revisar inventario, movimientos y pedidos sin entrar registro por registro.',
        'Un buen reporte depende de filtros correctos. Antes de sacar conclusiones, confirma centro, fechas, categoria y tipo de informacion.'
      ],
      modules: ['KPIs', 'Inventario', 'Movimientos', 'Pedidos', 'Filtros', 'Graficas', 'Tablas', 'Exportacion'],
      procedures: [
        {
          title: 'Preparar un reporte confiable',
          steps: [
            'Entra a Reportes y confirma el centro activo.',
            'Selecciona la pestana que responde tu pregunta: inventario, movimientos o pedidos.',
            'Define rango de fechas y filtros antes de interpretar resultados.',
            'Actualiza o recarga si los datos no corresponden al corte esperado.'
          ]
        },
        {
          title: 'Leer indicadores de inventario',
          steps: [
            'Revisa total de productos para entender volumen general.',
            'Identifica stock bajo y agotados como prioridades.',
            'Compara entradas contra salidas para ver consumo o demanda.',
            'Abre Inventario si necesitas corregir un producto especifico.'
          ]
        },
        {
          title: 'Analizar movimientos',
          steps: [
            'Filtra por periodo operativo: turno, dia, semana o mes.',
            'Observa productos con cambios bruscos de cantidad.',
            'Valida si las diferencias vienen de entrada, salida o ajuste.',
            'Consulta bitacora cuando un cambio no tenga explicacion clara.'
          ]
        },
        {
          title: 'Revisar pedidos y SLA',
          steps: [
            'Consulta pedidos creados, asignados, en ruta y entregados.',
            'Prioriza pedidos cercanos a vencimiento de SLA.',
            'Filtra por estado para limpiar la lista de trabajo.',
            'Si hay retraso, entra a Pedidos para revisar detalle y responsable.'
          ]
        }
      ],
      articles: [
        {
          title: 'Rotacion de productos',
          body: 'La rotacion se entiende comparando entradas, salidas y stock disponible. Un producto con muchas salidas y poco stock requiere seguimiento de reposicion. Un producto con mucho stock y pocas salidas puede estar ocupando espacio innecesario.'
        },
        {
          title: 'Desviaciones y anomalias',
          body: 'Una desviacion no siempre significa error. Puede ser una venta grande, una entrada atrasada, transferencia o ajuste por conteo. La regla practica es investigar cualquier cambio que no corresponda al comportamiento normal del centro.'
        },
        {
          title: 'Como compartir un reporte',
          body: 'Cuando compartas resultados, incluye centro, rango de fechas y filtros aplicados. Esto evita confusiones y permite que otra persona reproduzca el mismo reporte.'
        }
      ],
      fields: [
        ['Rango de fechas', 'Periodo que limita los datos del reporte.'],
        ['Centro', 'Bodega o unidad operativa analizada.'],
        ['Estado', 'Situacion actual del pedido o producto.'],
        ['KPI', 'Indicador resumido para detectar prioridad.'],
        ['Grafica', 'Visualizacion para encontrar tendencias o picos.']
      ],
      checks: ['Centro correcto', 'Fechas correctas', 'Filtros aplicados', 'KPIs interpretados', 'Desviaciones investigadas', 'Reporte compartido con contexto'],
      mistakes: ['Comparar periodos incompletos', 'Exportar sin filtros', 'Confundir stock total con disponibilidad por ubicacion', 'Sacar conclusiones sin revisar movimientos']
    },
    reglas: {
      label: 'Automatizacion',
      title: 'Configuracion de Reglas',
      subtitle: 'Manual para crear alertas, umbrales, responsables y procesos automaticos sin saturar al equipo.',
      image: 'linear-gradient(90deg, rgba(29,78,216,.92), rgba(22,163,74,.58)), url("https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=1500&q=80")',
      summary: [
        'Las reglas ayudan a detectar eventos importantes sin revisar todo manualmente. Pueden avisar stock bajo, pedidos atrasados, incidencias o condiciones repetitivas.',
        'Una regla bien configurada tiene origen claro, condicion medible, responsable asignado y accion definida. Si no cumple eso, puede generar ruido en lugar de ayudar.'
      ],
      modules: ['Umbrales', 'Alertas', 'Notificaciones', 'Responsables', 'Condiciones', 'Pruebas', 'Bitacora', 'Seguridad'],
      procedures: [
        {
          title: 'Definir el problema operativo',
          steps: [
            'Escribe que quieres detectar en una frase corta.',
            'Identifica el modulo que produce el dato: inventario, pedidos, camaras o sistema.',
            'Define por que el evento requiere accion.',
            'Confirma quien debe recibir la alerta.'
          ]
        },
        {
          title: 'Configurar condicion y umbral',
          steps: [
            'Selecciona el campo que se evaluara, por ejemplo stock disponible.',
            'Define operador y valor: menor que, mayor que, igual a o vencido.',
            'Evita condiciones demasiado amplias que disparen alertas constantes.',
            'Agrega excepciones si algunos productos o centros se manejan diferente.'
          ]
        },
        {
          title: 'Asignar accion',
          steps: [
            'Elige si se enviara notificacion, alerta, ticket o registro en bitacora.',
            'Asigna responsable principal y, si aplica, supervisor de respaldo.',
            'Define prioridad: informativa, media, alta o critica.',
            'Incluye mensaje claro con modulo, centro y accion esperada.'
          ]
        },
        {
          title: 'Probar y mantener reglas',
          steps: [
            'Antes de activar, prueba con un caso conocido.',
            'Revisa si la alerta llega al usuario correcto.',
            'Ajusta umbral si genera demasiados avisos.',
            'Programa revision periodica de reglas activas.'
          ]
        }
      ],
      articles: [
        {
          title: 'Ejemplo: alerta de stock bajo',
          body: 'Origen: Inventario. Condicion: stock disponible menor o igual al stock minimo. Accion: notificar al responsable de compras o almacen. Resultado esperado: el usuario revisa producto, valida consumo y genera reposicion si corresponde.'
        },
        {
          title: 'Ejemplo: pedido con SLA en riesgo',
          body: 'Origen: Pedidos. Condicion: pedido no entregado y fecha estimada cercana. Accion: alerta al supervisor logistico. Resultado esperado: reasignar, contactar transporte o actualizar estado.'
        },
        {
          title: 'Como evitar ruido operativo',
          body: 'No todo evento necesita alerta. Si una notificacion no provoca accion, probablemente sobra. Las reglas deben ser pocas, claras y revisadas por responsables reales.'
        }
      ],
      fields: [
        ['Origen', 'Modulo de donde se toma la informacion.'],
        ['Condicion', 'Regla logica que se evalua.'],
        ['Umbral', 'Valor limite que dispara la alerta.'],
        ['Accion', 'Respuesta automatica del sistema.'],
        ['Responsable', 'Usuario o rol que recibe seguimiento.']
      ],
      checks: ['Problema definido', 'Origen correcto', 'Condicion medible', 'Responsable asignado', 'Prueba realizada', 'Regla documentada'],
      mistakes: ['Crear reglas sin responsable', 'Usar umbrales muy sensibles', 'No probar antes de activar', 'Generar alertas duplicadas', 'No revisar reglas antiguas']
    },
    exportacion: {
      label: 'Datos y evidencia',
      title: 'Exportacion de Datos',
      subtitle: 'Manual para preparar, validar, descargar y compartir informacion en PDF o Excel sin perder trazabilidad.',
      image: 'linear-gradient(90deg, rgba(29,78,216,.92), rgba(8,145,178,.58)), url("https://images.unsplash.com/photo-1554224155-6726b3ff858f?auto=format&fit=crop&w=1500&q=80")',
      summary: [
        'Exportar datos sirve para auditoria, cierres de turno, analisis externo o solicitudes de direccion. El archivo descargado debe representar fielmente lo que se ve en el sistema.',
        'Antes de exportar, el usuario debe validar filtros, fechas, centro y columnas. Un archivo sin contexto puede causar decisiones equivocadas.'
      ],
      modules: ['PDF', 'Excel', 'Filtros', 'Columnas', 'Fechas', 'Auditoria', 'Reportes', 'Permisos'],
      procedures: [
        {
          title: 'Preparar la informacion',
          steps: [
            'Entra a Reportes o al modulo que permita descarga.',
            'Selecciona centro activo y rango de fechas.',
            'Aplica filtros de categoria, estado o tipo de movimiento.',
            'Revisa que la tabla muestre solo los registros necesarios.'
          ]
        },
        {
          title: 'Elegir formato correcto',
          steps: [
            'Usa PDF cuando el archivo se presentara como evidencia o resumen.',
            'Usa Excel cuando el destinatario necesita filtrar, ordenar o analizar.',
            'No edites manualmente el archivo si sera considerado evidencia oficial.',
            'Si debes hacer analisis, conserva tambien el archivo original.'
          ]
        },
        {
          title: 'Nombrar y compartir',
          steps: [
            'Usa nombres con modulo, centro y fecha.',
            'Ejemplo: reportes-inventario-bodega-norte-2026-07-07.xlsx.',
            'Comparte solo con destinatarios autorizados.',
            'Incluye en el mensaje los filtros usados para generar el archivo.'
          ]
        },
        {
          title: 'Controlar informacion sensible',
          steps: [
            'Verifica si el archivo contiene datos personales, clientes o costos.',
            'Evita enviar archivos por canales no oficiales.',
            'Si el archivo se corrige, genera una nueva version y conserva registro.',
            'Reporta exportaciones erroneas al supervisor si ya fueron enviadas.'
          ]
        }
      ],
      articles: [
        {
          title: 'PDF vs Excel',
          body: 'PDF es ideal cuando se necesita una fotografia fija del reporte. Excel es mejor para analisis y revision detallada. Elegir formato incorrecto puede provocar retrabajo o perdida de contexto.'
        },
        {
          title: 'Filtros que siempre deben revisarse',
          body: 'Centro, fechas, categoria, estado y tipo de movimiento son filtros criticos. Si cualquiera esta mal, el reporte puede mezclar informacion de otras bodegas o periodos.'
        },
        {
          title: 'Buenas practicas de evidencia',
          body: 'Guarda el archivo con fecha, no cambies datos manualmente y conserva el criterio de extraccion. Si el archivo alimenta auditoria, el origen debe poder reconstruirse desde PEIA.'
        }
      ],
      fields: [
        ['PDF', 'Formato fijo para compartir evidencia visual.'],
        ['Excel', 'Formato editable para analisis controlado.'],
        ['Filtros', 'Criterios que limitan los datos exportados.'],
        ['Columnas', 'Campos incluidos en el archivo final.'],
        ['Version', 'Identificador para distinguir cortes o correcciones.']
      ],
      checks: ['Filtros revisados', 'Formato correcto', 'Columnas validadas', 'Nombre de archivo claro', 'Destinatario autorizado', 'Archivo original conservado'],
      mistakes: ['Exportar toda la base sin necesidad', 'Enviar datos de otro centro', 'Modificar datos oficiales en Excel', 'No indicar filtros al compartir', 'Usar nombres de archivo genericos']
    },
    videos: {
      label: 'Capacitacion',
      title: 'Video Tutoriales',
      subtitle: 'Biblioteca preparada para publicar videos de capacitacion, con imagenes temporales mientras se reciben los archivos finales.',
      image: 'linear-gradient(90deg, rgba(29,78,216,.92), rgba(220,38,38,.48)), url("https://images.unsplash.com/photo-1552664730-d307ca884978?auto=format&fit=crop&w=1500&q=80")',
      isVideo: true,
      summary: [
        'Esta seccion esta pensada para que el equipo coloque videos reales cuando esten listos. Por ahora muestra tarjetas con imagenes, duracion estimada, nivel y descripcion del flujo que debe grabarse.',
        'Cada video debe resolver una tarea concreta. Lo ideal es mantenerlos cortos, mostrar el proceso completo y cerrar con una validacion final dentro del sistema.'
      ],
      modules: ['Primer ingreso', 'Inventario', 'Pedidos', 'Reportes', 'Reglas', 'Exportacion', 'Errores comunes', 'Capacitacion'],
      videos: [
        {
          title: 'Primer ingreso y seleccion de centro',
          level: 'Basico',
          duration: '4 min',
          image: 'https://images.unsplash.com/photo-1551434678-e076c223a692?auto=format&fit=crop&w=900&q=80',
          description: 'Recorrido desde login hasta confirmacion de usuario, centro activo y menu lateral.'
        },
        {
          title: 'Alta de producto y stock minimo',
          level: 'Inventario',
          duration: '6 min',
          image: 'https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?auto=format&fit=crop&w=900&q=80',
          description: 'Demostracion para crear un SKU, asignar categoria, ubicacion y regla de stock bajo.'
        },
        {
          title: 'Registrar entrada y salida',
          level: 'Operacion',
          duration: '7 min',
          image: 'https://images.unsplash.com/photo-1600880292203-757bb62b4baf?auto=format&fit=crop&w=900&q=80',
          description: 'Flujo completo de movimiento con cantidad, motivo, referencia y revision del historial.'
        },
        {
          title: 'Leer reportes y detectar desviaciones',
          level: 'Supervisor',
          duration: '8 min',
          image: 'https://images.unsplash.com/photo-1551288049-bebda4e38f71?auto=format&fit=crop&w=900&q=80',
          description: 'Interpretacion de KPIs, filtros, graficas y decisiones a partir de movimientos.'
        },
        {
          title: 'Exportar datos para auditoria',
          level: 'Reportes',
          duration: '5 min',
          image: 'https://images.unsplash.com/photo-1554224155-6726b3ff858f?auto=format&fit=crop&w=900&q=80',
          description: 'Preparacion de filtros, eleccion de PDF o Excel y recomendaciones de envio.'
        },
        {
          title: 'Configurar alerta de stock bajo',
          level: 'Administrador',
          duration: '6 min',
          image: 'https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=900&q=80',
          description: 'Ejemplo de regla con origen, condicion, responsable, accion y prueba.'
        }
      ],
      procedures: [
        {
          title: 'Como publicar un video cuando este listo',
          steps: [
            'Solicita el archivo final en formato MP4 o un enlace interno autorizado.',
            'Sustituye la imagen temporal de la tarjeta por el reproductor de video.',
            'Mantén titulo, nivel, duracion y descripcion para que el usuario sepa que aprendera.',
            'Prueba que el video cargue en navegadores usados por la empresa.'
          ]
        },
        {
          title: 'Estructura recomendada para grabar',
          steps: [
            'Inicia explicando la tarea y el modulo que se usara.',
            'Muestra los campos obligatorios y errores comunes.',
            'Realiza el flujo completo sin saltos importantes.',
            'Cierra mostrando como confirmar que la accion quedo guardada.'
          ]
        }
      ],
      articles: [
        {
          title: 'Criterios de calidad para capacitacion',
          body: 'Un buen tutorial debe durar entre 2 y 8 minutos, enfocarse en una sola tarea y usar datos de ejemplo. Si muestra demasiados temas a la vez, el trabajador no sabra que recordar.'
        },
        {
          title: 'Proteccion de datos en grabaciones',
          body: 'No deben verse contrasenas, datos reales de clientes, costos sensibles o informacion personal. Antes de publicar, revisa el video completo y valida que no haya informacion no autorizada.'
        }
      ],
      fields: [
        ['Titulo', 'Debe indicar la tarea exacta que se aprendera.'],
        ['Duracion', 'Ayuda al usuario a elegir segun tiempo disponible.'],
        ['Nivel', 'Basico, operativo, supervisor o administrador.'],
        ['Estado', 'Pendiente, en revision o publicado.']
      ],
      checks: ['Video con tema unico', 'Datos sensibles ocultos', 'Duracion razonable', 'Validacion final visible', 'Enlace probado'],
      mistakes: ['Videos demasiado largos', 'Mezclar varios procesos', 'No mostrar resultado final', 'Usar datos reales sensibles', 'Publicar sin revisar audio o imagen']
    }
  };

  const topic = topics[topicKey] || topics.intro;
  const qs = id => document.getElementById(id);

  document.title = `PEIA - ${topic.title}`;
  qs('detailHero').style.backgroundImage = topic.image;
  qs('detailLabel').textContent = topic.label;
  qs('detailTitle').textContent = topic.title;
  qs('detailSubtitle').textContent = topic.subtitle;
  qs('detailSummary').innerHTML = topic.summary.map(item => `<p>${item}</p>`).join('');
  qs('moduleTags').innerHTML = topic.modules.map(item => `<span>${item}</span>`).join('');
  qs('fieldList').innerHTML = topic.fields.map(([name, text]) => `
    <article class="manual-field">
      <strong>${name}</strong>
      <p>${text}</p>
    </article>
  `).join('');
  qs('detailArticles').innerHTML = topic.articles.map(article => `
    <article class="manual-article">
      <h3>${article.title}</h3>
      <p>${article.body}</p>
    </article>
  `).join('');
  qs('detailProcedures').innerHTML = topic.procedures.map((procedure, index) => `
    <article class="manual-procedure">
      <div class="manual-procedure-head">
        <span>${index + 1}</span>
        <h3>${procedure.title}</h3>
      </div>
      <ol>
        ${procedure.steps.map(step => `<li>${step}</li>`).join('')}
      </ol>
    </article>
  `).join('');
  qs('detailChecklist').innerHTML = topic.checks.map(item => `<li>${item}</li>`).join('');
  qs('detailMistakes').innerHTML = topic.mistakes.map(item => `<li>${item}</li>`).join('');

  const videoSection = qs('videoLibrary');
  if (topic.isVideo) {
    videoSection.hidden = false;
    videoSection.innerHTML = topic.videos.map(video => `
      <article class="video-card">
        <div class="video-thumb" style="background-image: linear-gradient(180deg, rgba(15,23,42,.05), rgba(15,23,42,.62)), url('${video.image}')">
          <span class="video-play">
            <svg width="22" height="22" viewBox="0 0 24 24" fill="currentColor"><path d="M8 5v14l11-7z"/></svg>
          </span>
          <span class="video-state">Espacio reservado para video</span>
        </div>
        <div class="video-body">
          <div class="video-meta"><span>${video.level}</span><span>${video.duration}</span></div>
          <h3>${video.title}</h3>
          <p>${video.description}</p>
        </div>
      </article>
    `).join('');
  }

  qs('printGuideDetail').addEventListener('click', () => window.print());
});

function renderGuideShell() {
  const root = document.getElementById('guideDetailRoot');
  if (!root) return;

  root.innerHTML = `
    <div class="app-layout">
      <aside class="sidebar" id="sidebar">
        <div class="sidebar-brand">
          <div class="brand-icon">
            <svg width="26" height="26" viewBox="0 0 28 28" fill="none">
              <rect x="2" y="8" width="10" height="10" rx="2" fill="#fff"/>
              <rect x="16" y="2" width="10" height="10" rx="2" fill="#fff"/>
              <rect x="10" y="16" width="10" height="10" rx="2" fill="rgba(255,255,255,.6)"/>
            </svg>
          </div>
          <span>PEIA</span>
        </div>
        <nav class="sidebar-nav">
          <ul>
            <li><a href="inicio.html"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/></svg>Inicio</a></li>
            <li><a href="inventario.html"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 7H4a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2z"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>Inventario</a></li>
            <li><a href="pedidos.html"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2"/><rect x="9" y="3" width="6" height="4" rx="1"/><line x1="9" y1="12" x2="15" y2="12"/><line x1="9" y1="16" x2="12" y2="16"/></svg>Pedidos</a></li>
            <li><a href="reportes.html"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>Reportes</a></li>
            <li><a href="roles.html"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>Roles</a></li>
            <li><a href="camaras.html"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"/><circle cx="12" cy="13" r="4"/></svg>Camaras</a></li>
            <li><a href="prediccion.html"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg>Prediccion</a></li>
            <li><a href="notificaciones.html" class="has-badge"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 0 1-3.46 0"/></svg>Notificaciones<span class="badge">3</span></a></li>
            <li><a href="usuarios.html"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>Usuarios</a></li>
            <li><a href="configuracion.html"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="3"/><path d="M19.07 4.93a10 10 0 0 1 0 14.14M4.93 4.93a10 10 0 0 0 0 14.14"/></svg>Configuracion</a></li>
            <li><a href="bitacora.html"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z"/><path d="M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z"/></svg>Bitacora</a></li>
            <li class="active"><a href="guia.html"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M9.09 9a3 3 0 1 1 5.82 1c-.82 1.31-2.91 1.18-2.91 3"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>Guia</a></li>
          </ul>
        </nav>
        <div class="sidebar-footer">
          <button class="btn-logout" id="btnLogout">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>
            Cerrar sesion
          </button>
        </div>
      </aside>
      <main class="main-area">
        <header class="topbar">
          <div class="topbar-left">
            <div class="warehouse-selector">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>
              <span id="activeCentro">Bodega Norte</span>
              <svg class="chevron-sm" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 9l6 6 6-6"/></svg>
              <div class="warehouse-dropdown" id="warehouseDropdown">
                <button class="warehouse-opt active" data-id="norte">Bodega Norte</button>
                <button class="warehouse-opt" data-id="sur">Bodega Sur</button>
              </div>
            </div>
          </div>
          <div class="topbar-right">
            <button class="topbar-btn notif-btn" aria-label="Notificaciones">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 0 1-3.46 0"/></svg>
              <span class="notif-dot">3</span>
            </button>
            <div class="user-menu" id="userMenu">
              <div class="avatar" id="userAvatar">AG</div>
              <span class="user-name" id="userName">Ariel Guevara</span>
              <svg class="chevron-sm" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 9l6 6 6-6"/></svg>
            </div>
          </div>
        </header>
        <div class="page-content guide-detail-page">
          <section class="manual-hero" id="detailHero">
            <a class="guide-back" href="guia.html">Volver a guia</a>
            <span id="detailLabel"></span>
            <h1 id="detailTitle"></h1>
            <p id="detailSubtitle"></p>
          </section>
          <section class="manual-overview">
            <div class="manual-summary">
              <h2>Resumen operativo</h2>
              <div id="detailSummary"></div>
            </div>
            <div class="manual-modules">
              <h2>Herramientas cubiertas</h2>
              <div id="moduleTags"></div>
            </div>
          </section>
          <section class="manual-layout">
            <article class="manual-main">
              <div class="manual-block">
                <h2>Procedimientos paso a paso</h2>
                <div id="detailProcedures"></div>
              </div>
              <div class="manual-block" id="videoLibrary" hidden></div>
              <div class="manual-block">
                <h2>Articulos de uso</h2>
                <div class="manual-article-grid" id="detailArticles"></div>
              </div>
              <div class="manual-block">
                <h2>Campos y conceptos importantes</h2>
                <div class="manual-field-grid" id="fieldList"></div>
              </div>
            </article>
            <aside class="manual-aside">
              <button class="btn btn-secondary" id="printGuideDetail">Imprimir guia</button>
              <div class="manual-card">
                <h2>Checklist final</h2>
                <ul id="detailChecklist"></ul>
              </div>
              <div class="manual-card">
                <h2>Errores comunes</h2>
                <ul id="detailMistakes"></ul>
              </div>
              <div class="manual-card manual-support">
                <h2>Cuando pedir apoyo</h2>
                <p>Solicita apoyo si no tienes permisos, el centro activo no aparece, un dato no coincide con la operacion fisica o una accion puede afectar reportes oficiales.</p>
                <a class="btn btn-primary" href="guia.html">Centro de ayuda</a>
              </div>
            </aside>
          </section>
        </div>
      </main>
    </div>
  `;
}
