Ainda n�o temos o Puppet Master completamente operacional.

� preciso come�ar uma inst�ncia de CentralDirectory e v�rias de servidores (depois de ter iniciado o PuppetMaster). 
Estas v�o-se registar no PuppetMaster e basta seleccionar os servidores e fazer start � medida que queremos os servidores inicializados.
Exixte um ConsoleClient para popular as tabelas.

Na vers�o para o checkpoint temos as seguintes principais funcionalidades:

- Arquitectura geral do sistema em Semitabelas, em que cada uma � respons�vel por um determinado n�mero de chaves limitado pelas suas hashs (ver o relat�rio)
- Algor�tmo para inser��o de novo n� (em que o sistema detecta os servidores mais carregados e divide as suas cargas no novo n�)
- Replica��o
- Fun��es de GET e PUT com diferentes vers�es (K), mas sem 2PC nem locks