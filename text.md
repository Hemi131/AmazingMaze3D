# Světla

Otevřete si výchozí scénu a prohlédněte si strukturu. Přešli jsme na GameWindow a notně ořezali části, které jsme používali na začátku. Mnohé věci už znáte z minulých cvičení, jako např. `Viewport`, `Cameru` nebo podstatně zeštíhlený `Model`.

Přibylo nám mimo jiné jednoduché načítání souboru typu `obj`. Jedná se pouze o naprosté minimum a nebude fungovat na žádné složitější objekty. Stačí nám nicméně k načtení základních objektů. 

## Normály

Než se začneme vůbec zabývat světly, musíme připravit normály. Samotný výpočet normál proveďte ve třídě `SimpleObj` při načítání. Pro začátek si můžeme vystačit s výpočtem normály ve vrcholu jako obyčejným znormalizováním součtu normál všech přilehlých ploch. 

Doplňte strukturu `Vertex` o další `Vector3` pro normály.

Napište metodu `(Vertex[] vertices, int[] index)`, která pro každý trojúhelník spočítá jeho normálu a přičte ji do výsledného pole normál. Na konci všechny normály znormalizuje.

Upravte `Model`, aby normály správně použil.

Dále upravte vertex shader, aby data o normálách použil a poslal je, zatím netransformované do fragment shaderu, kde podle nich obarvěte objekt tak, že normálu vydělíte 2 a přičtete 0,5. Tím ji přemapujete z intervalu <-1, 1> na <0, 1>, takže pokryje barevný rozsah.

Zamyslete se nad barvami, kterými se obarvil objekt. Dokážete si je odůvodnit? Přepněte si případně vykreslování z drátěného modelu na vyplněný.

## Světlo

Připravte třídu `Light`, která bude popisovat obecný světelný zdroj. Využijeme faktu, že rozdíl mezi konkrétním umístěním světla v prostoru a směrem světla z "nekonečna" lze snadno matematicky popsat. 

Kromě `Vector4` pozice přidejte ještě `Vector3` barvu a `float` intenzitu. Oddělení barvy od intenzity nám dá lepší kontrolu nad světlem (při zachování barvy můžeme upravovat jeho intenzitu). 

Konstruktor může vypadat například `Light(Vector3 posOrDir, bool directional)`.

## Budiž světlo! 

Nyní bude třeba upravit shadery, aby se světlem začaly počítat. Ještě předtím však musíme dostat světlo k modelu. To můžeme pro jednoduchost udělat tak, že při volání `Draw` kromě kamery přidáme i odkaz světlo.

*Pro více objektů a světel by se nám asi hodila nějaká třída pro správu Scény, která se o předávání parametrů bude starat.*

Světla můžeme počítat v různých souřadných systémech, od modelu, přes světové souřadnice, view, ale třeba také v normálovém prostoru (o kterém si ještě řekneme později). V zásadě to nejsou velké rozdíly, jen je třeba si to na začátku dobře ujasnit a pak se držet jednoho způsobu. 

Asi nejuniverzálnější se jeví v tuto chvíli world space. Kromě pozice a vlastností světla však bude třeba do shaderu dostat i pozici kamery. Začněme s jednoduchý směrovým světlem (tedy w = 0).

Upravte fragment shader tak, aby jako uniform umožnil předat parametry světla (pozici, barvu i intenzitu). Z vertex shaderu musí do fragment shaderu přijít normály transformované do světových souřadnic (`transpose( inverse(...))`), a souřadnice vrcholu (nebo spíše fragmentu) ve světových souřadnicích.

Můžete přidat ambientní osvětlení buď přidáním uniform, nebo jednoduše přičít hodnotu. Nezapomeňte hodnotu difuzního světla omezit, aby nešla do záporné hodnoty.

*Shadery obecně nemají moc rád větvení, ale obsahují celou řadu funkcí, které mohou pomoci. Místo testování na 0 a případné omezení, je mnohem vhodnější použít funkci min, která umí případně pracovat i s vektory.*

Pokud vše funguje, můžete se podívat na homogenní souřadnici světla a v případě nenulové hodnoty (tady bude tam podmínka asi na místě) vypočítat světlo bodové (můžete přidat uniform popisující útlum světla, viz přednáška - 1/(a + bx + cx^2)).

## Bída a lesk

Poslední úpravu, kterou provedeme, bude zavedení spekulární (tedy lesklé) složky materiálu/světla. 

Začneme úpravou fragment shaderu, kde vytvoříme strukturu pro uchování vlastností materiálu:
```glsl
struct Material {
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float shininess;
};
uniform Material material;
````
*Podobnou strukturu můžete použít i pro světlo pro větší přehlednost.*

K takové uniform proměnné pak lze na straně C# přistupovat přes jméno s  tečkovou notací, podobně, jako v normálním jazyce.

Vytvořte třídu `Material`, která bude uchovávat difuzní a spekulární barvu a také koeficient "ostrosti" odlesku. Pro oddělení kódu a celkové zpřehlednění můžeme udělat nastavení uniforms přímo materiálu, např. v metodě `SetUniforms(Shader)`. Materiál přidejte jako vlastnost modelu. 

Kromě materiálu bude také třeba předat pozici kamery ve světových souřadnicích pro výpočet odlesku. 

Ve fragment shaderu pak postupně vypočítám ambientní osvětlení, difuzní a spekulární a všechny složky posčítáme do výsledné barvy.