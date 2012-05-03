/////		PADITable: Projecto de PADI IST-Taguspark 2011 MERC-MEIC 		//////////
////        Bernardo Sim�es;   Afonso Oliveira;     Rui Francisco           /////////


0. Quick Start

1�Correr o programa "PuppetMaster->bin->Release->PuppetMaster.exe"
2�dar permiss�es na firewall
3� Fazer duplo click num script listado por debaixo de "User Scripts"
4� Ver montes de clientes/clientes e servidores a serem lan�ados e a morrer
5� Ir � tab "Clients & Server Commands" e ver os clientes que est�o a correr!
Se clicar "DUMP Client" pode ver o conteudo dos registos dos clientes.
6� Escreva o valor de uma key a frente do bota�o "GET ALL" carregue no bot�o
e ir� ver uma tabela com os valores da KEY nos v�rios servidores e com v�rios
Timestamps
7� Voltar a fazer duplo click num script, ver todos os clientes/servidores que estavam 
a correr a morrer e ver os novos a serem lan�ados.


1.Como Correr o Projecto

2.Scripts
2.1 Adicionar Novos Scripts
2.2 Mostrar Conteudo do Script
2.3 Correr um Script
2.3.1 Correr um script passo a passo
2.4 Consequencias de correr o script
2.4.1 Verificar Clientes e Servidores que est�o a correr
2.4.2 Comandar Clientes que Est�o a Correr
2.4.3 Fazer GETAll a uma key

3.Iniciar Clientes\Servidores e Central Diresctory
3.1 Iniciar\Matar Cliente
3.2 Iniciar\Matar Servidores
3.3. Iniciar\Matar Central Directory
3.4 Matar Tudo (menos o puppet master)

4. Algumas notas importantes

1.Como Correr
1.1 Pr� requesitos:
 Sistema Operativo: Windows
 Porto 9090 aberto e 8090
 Dar permiss�es � sua  firewall para correr os programas "PuppetMaster.exe","Client.exe" ,"CentralDirectory.exe" e "Server.exe"

 1.2 Correr
 Para correr o projecto lance o "puppet master" fazendo duplo "click":
     "PuppetMaster->bin->Release->PuppetMaster.exe"
	 
2.Scripts
O PuppetMaster ao ser iniciado adiciona todos os scripts contidos dentro da directoria "Scripts",
os scrits tem de ser do formato ".txt" e s�o reconhecidos scripts dentro de sub-directorias

2.1 Adicionar Novos Scripts
Para adicionar um script clickar no but�o "Adicionar Novo Script" ou ent�o adicionar os scripts � pasta "Scripts"

2.2 Mostrar Conteudo do Script
Assim que um Scrit � seleccionado, o seu conteudo � mostrado na ListBox a baixo da ListBox de selec��o
 de Scripts

2.3 Correr um Script
Para correr um script basta:
a) Seleccionar um script
b1) Fazer duplo click no script seleccionado
                  OU
b2) Carregar no bot�o correr script

2.3.1 Correr um script passo a passo
Para correr um script passo a passo basta seleccionar o script na lista de scripts, 
na listbox que faz display do script escolher o pa�o que quer efectuar e carregar no bot�o
"Run Step". Assim que a instru��o � executada o Puppet Master passa automaticamente para a 
instru��o a seguir e fica � espera que se carregue "Run Step" para prosseguir.


2.4 Consequencias de correr o script

2.4.1 Verificar Clientes e Servidores que est�o a correr
Depois de se correr um script � possivel visualizar o nome dos Clientes e Servidores nas ListBoxes chamadas 
de clientes e servidores.

2.4.2 Comandar Clientes que Est�o a Correr
Na tab "clients&servers" � poss�vel ver os clientes que est�o a correr (nota: cas estejam muitos clientes a correr
fa�a scroll left). Se carregar no bot�o "DUMP CLIENT" o cliente mostrar� o conteudo dos seus registos. Tambem podem
ser executadas opera��es directamente nos clientes a partir deste menu

2.4.3 Fazer GETAll a uma key
Na tab "Clients & Servers" na textbox a seguir ao bot�o GETALL insere-se a string da KEY.
Se a KEY estiver contida no servidor o "Puppet Master" ir� gerar uma tabelas com os par�metros
caracter�sticos da KEY


3.Iniciar Clientes\Servidores e Central Diresctory
� Poss�vel iniciar clientes\servidores e Central Directory sem ser necess�rio utilizar Scripts

3.1 Iniciar\Matar Clientes
Para iniciar clientes basta carregar no bot�o "start client", se um nome para o cliente for adicionado o cliente ter� 
esse nome, caso contr�rio o sistema atribuir� um nome ao cliente. Para matar um cliente basta selecciona-lo na listBox 
e carregar no bot�o "Kill Client"

3.2Iniciar\Matar Servidores
Clicar start server, um numero � adicionado ao servidor automaticamente. Para matar um servidor basta seleccionar o 
 servidor que se deseja matar e clicar no bot�o "Kill Server"
 
3.3. Iniciar\Matar Central Directory
Para iniciar o central directory carregar no bot�o "Start CD", para o matar carregar no bot�o "Start CD"

3.4 Matar Tudo (menos o puppet master)
Para matar todos os processos lan�ados pelo puppet master carregar no bot�o "Kill All". Todas as vari�veis
(numero de clientes/servidores, clientes na tab "Clients & Servers" etc...) ser�o reiniciadas.


4. Algumas notas importantes
-O Central Directory corre sempre no porto 9090
-Os scripts nao precisam de ter o IP + Porto do cliente ou servidor. At� � conveniente que este n�o seja especificado pois 
por vezes o windows demora um pouco a perceber que os portos est�o disponiveis (mesmo que os clientes se desregistem dos mesmos)
o que cria por vezes problemas a correr v�rios scripts.
-Caso o programa crashe � 99% prov�vel que seja por causa de o porto 9090 esteja a ser usado. Respire fundo e reinicio o "Puppet Master"
