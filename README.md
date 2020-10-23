# LABORATORIO #4

***Kevin Romero      1047519***

***José De León      1072619***

---

### **Objetivos:**

- Aplicar los conceptos de la compresión de LZW
- Aplicar los conceptos de razones de compresión

---

### Contenido

- Implementación de una interfaz de compresión para la compresión LZW
- Implementación de la compresión LZW para strings en un proyecto de consola
- Implementación de la compresión LZW para archivos de texto en una API

---

### Uso del Proyecto de Consola
Al compilar la clase ***program*** observará el resultado de la compresión y descompresión LZW para una frase previamente ingresada.

Así mismo podrá observar la ***razón de compresión***, ***factor de compresión*** y el ***porcentaje de reducción*** para la compresión de la cadena ingresada.

---

### Uso de la API

1. Haga una petición **POST** en ***api/compress/{name}*** agregando en el apartado form-data el **archivo de texto** a comprimir, la llave con la que se ingresa el archivo deberá llamarse **file**, tambien deberá intercambiar  el parámetro de la petición por el nombre con el que desea que se guarde la compresión
***(ej: api/compress/compresion1)***

El archivo comprimido resultante lo podrá encontrar en la solución del proyecto dentro de la carpeta ***Data/compressions*** con el nombre previamente ingresado y con la extension **.lzw**

2. Haga una petición **POST** en ***api/decompress*** agregando en el apartado form-data **el archivo .lzw** a descomprimir, la llave con la que se ingresa el archivo deberá llamarse **file**.

El archivo descomprimido resultante lo podrá encontrar en la solución del proyecto dentro de la carpeta ***Data/decompressions*** con el nombre original y en formato de texto plano.

3. Haga una petición **GET** en ***api/compressions*** para consultar todas las compresiones realizadas en la api. Esta consulta devuelve una serie de objetos json con propiedades como la razón de compresión, factor de compresión y el porcentaje de reducción, además del nombre original del archivo compreso y la ruta del archivo resultante.

Los registros de estas compresiones son almacenadas en el archivo ***Data/compressions_history.json*** dentro de la solucion del proyecto.
