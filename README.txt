/////		PADITable: Projecto de PADI IST-Taguspark 2011 MERC-MEIC 		//////////
////        Bernardo Simões;   Afonso Oliveira;     Rui Francisco           /////////


0. Quick Start

1ºCorrer o programa "PuppetMaster->bin->Release->PuppetMaster.exe"
2ºdar permissões na firewall
3º Fazer duplo click num script listado por debaixo de "User Scripts"
4º Ver montes de clientes/clientes e servidores a serem lançados e a morrer
5º Ir à tab "Clients & Server Commands" e ver os clientes que estão a correr!
Se clicar "DUMP Client" pode ver o conteudo dos registos dos clientes.
6º Escreva o valor de uma key a frente do botaão "GET ALL" carregue no botão
e irá ver uma tabela com os valores da KEY nos vários servidores e com vários
Timestamps
7º Voltar a fazer duplo click num script, ver todos os clientes/servidores que estavam 
a correr a morrer e ver os novos a serem lançados.


1.Como Correr o Projecto

2.Scripts
2.1 Adicionar Novos Scripts
2.2 Mostrar Conteudo do Script
2.3 Correr um Script
2.3.1 Correr um script passo a passo
2.4 Consequencias de correr o script
2.4.1 Verificar Clientes e Servidores que estão a correr
2.4.2 Comandar Clientes que Estão a Correr
2.4.3 Fazer GETAll a uma key

3.Iniciar Clientes\Servidores e Central Diresctory
3.1 Iniciar\Matar Cliente
3.2 Iniciar\Matar Servidores
3.3. Iniciar\Matar Central Directory
3.4 Matar Tudo (menos o puppet master)

4. Algumas notas importantes

1.Como Correr
1.1 Pré requesitos:
 Sistema Operativo: Windows
 Porto 9090 aberto e 8090
 Dar permissões à sua  firewall para correr os programas "PuppetMaster.exe","Client.exe" ,"CentralDirectory.exe" e "Server.exe"

 1.2 Correr
 Para correr o projecto lance o "puppet master" fazendo duplo "click":
     "PuppetMaster->bin->Release->PuppetMaster.exe"
	 
2.Scripts
O PuppetMaster ao ser iniciado adiciona todos os scripts contidos dentro da directoria "Scripts",
os scrits tem de ser do formato ".txt" e são reconhecidos scripts dentro de sub-directorias

2.1 Adicionar Novos Scripts
Para adicionar um script clickar no butão "Adicionar Novo Script" ou então adicionar os scripts à pasta "Scripts"

2.2 Mostrar Conteudo do Script
Assim que um Scrit é seleccionado, o seu conteudo é mostrado na ListBox a baixo da ListBox de selecção
 de Scripts

2.3 Correr um Script
Para correr um script basta:
a) Seleccionar um script
b1) Fazer duplo click no script seleccionado
                  OU
b2) Carregar no botão correr script

2.3.1 Correr um script passo a passo
Para correr um script passo a passo basta seleccionar o script na lista de scripts, 
na listbox que faz display do script escolher o paço que quer efectuar e carregar no botão
"Run Step". Assim que a instrução é executada o Puppet Master passa automaticamente para a 
instrução a seguir e fica à espera que se carregue "Run Step" para prosseguir.


2.4 Consequencias de correr o script

2.4.1 Verificar Clientes e Servidores que estão a correr
Depois de se correr um script é possivel visualizar o nome dos Clientes e Servidores nas ListBoxes chamadas 
de clientes e servidores.

2.4.2 Comandar Clientes que Estão a Correr
Na tab "clients&servers" é possível ver os clientes que estão a correr (nota: cas estejam muitos clientes a correr
faça scroll left). Se carregar no botão "DUMP CLIENT" o cliente mostrará o conteudo dos seus registos. Tambem podem
ser executadas operações directamente nos clientes a partir deste menu

2.4.3 Fazer GETAll a uma key
Na tab "Clients & Servers" na textbox a seguir ao botão GETALL insere-se a string da KEY.
Se a KEY estiver contida no servidor o "Puppet Master" irá gerar uma tabelas com os parâmetros
característicos da KEY


3.Iniciar Clientes\Servidores e Central Diresctory
É Possível iniciar clientes\servidores e Central Directory sem ser necessário utilizar Scripts

3.1 Iniciar\Matar Clientes
Para iniciar clientes basta carregar no botão "start client", se um nome para o cliente for adicionado o cliente terá 
esse nome, caso contrário o sistema atribuirá um nome ao cliente. Para matar um cliente basta selecciona-lo na listBox 
e carregar no botão "Kill Client"

3.2Iniciar\Matar Servidores
Clicar start server, um numero é adicionado ao servidor automaticamente. Para matar um servidor basta seleccionar o 
 servidor que se deseja matar e clicar no botão "Kill Server"
 
3.3. Iniciar\Matar Central Directory
Para iniciar o central directory carregar no botão "Start CD", para o matar carregar no botão "Start CD"

3.4 Matar Tudo (menos o puppet master)
Para matar todos os processos lançados pelo puppet master carregar no botão "Kill All". Todas as variáveis
(numero de clientes/servidores, clientes na tab "Clients & Servers" etc...) serão reiniciadas.


4. Algumas notas importantes
-O Central Directory corre sempre no porto 9090
-Os scripts nao precisam de ter o IP + Porto do cliente ou servidor. Até é conveniente que este não seja especificado pois 
por vezes o windows demora um pouco a perceber que os portos estão disponiveis (mesmo que os clientes se desregistem dos mesmos)
o que cria por vezes problemas a correr vários scripts.
-Caso o programa crashe é 99% provável que seja por causa de o porto 9090 esteja a ser usado. Respire fundo e reinicio o "Puppet Master"
