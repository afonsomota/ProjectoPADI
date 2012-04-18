Ainda não temos o Puppet Master completamente operacional.

É preciso começar uma instância de CentralDirectory e várias de servidores (depois de ter iniciado o PuppetMaster). 
Estas vão-se registar no PuppetMaster e basta seleccionar os servidores e fazer start à medida que queremos os servidores inicializados.
Exixte um ConsoleClient para popular as tabelas.

Na versão para o checkpoint temos as seguintes principais funcionalidades:

- Arquitectura geral do sistema em Semitabelas, em que cada uma é responsável por um determinado número de chaves limitado pelas suas hashs (ver o relatório)
- Algorítmo para inserção de novo nó (em que o sistema detecta os servidores mais carregados e divide as suas cargas no novo nó)
- Replicação
- Funções de GET e PUT com diferentes versões (K), mas sem 2PC nem locks