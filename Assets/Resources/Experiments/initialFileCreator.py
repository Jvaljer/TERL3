group = 6
groupMove = [["drag","TP","joy"],["TP","drag","joy"],["joy","TP","drag"],
            ["drag","joy","TP"],["TP","joy","drag"],["joy","drag","TP"]]
#cardList = [12,28,57,2,20,15,37,50,36,9,22,47]
cardList = [22, 37, 11, 8, 47, 51, 34, 29, 59, 43, 6, 10, 31, 25, 14, 19, 
            40, 56, 24, 39, 2, 17, 54, 49, 26, 32, 4, 0, 58, 42, 15, 1, 
           23, 38, 45, 50, 12, 9, 36, 30, 53, 57, 13, 7, 33, 28, 48, 52]
lenght = (int) (len(cardList)/3)
training = ""
wall = ""

file = open("initialTrialFile.txt", "w")
file.write("Group;Participant;CollabEnvironememn;trialNb;training;MoveMode;wall;CardToTag;\n");
nb = 0
for g in range(1, group+1):
    nb = 0
    for n in range(0, lenght):
        if cardList[n] <20:
            wall = "L"
        elif cardList[n] < 40:
            wall = "B"
        else:
            wall = "R"

        if n < 6:
            training = "1"
        else:
            training = "0"
        if n%2 == 0:
            file.write("g0" + str(g) + ";p01;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][0] + ";" + "move;" + wall + ";" + str(cardList[n]) + ";\n")
            file.write("g0" + str(g) + ";p02;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][0] + ";" + "search;" + wall + ";" + str(cardList[n]) + ";\n")
            nb += 1
        else:
            file.write("g0" + str(g) + ";p01;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][0] + ";" + "search;" + wall + ";" + str(cardList[n]) + ";\n")
            file.write("g0" + str(g) + ";p02;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][0] + ";" + "move;" + wall + ";" + str(cardList[n]) + ";\n")
            nb += 1
    for n in range(0, lenght-6):
        if cardList[lenght-7-n] <20:
            wall = "L"
        elif cardList[lenght-7-n] < 40:
            wall = "B"
        else:
            wall = "R"

        training = "0"
        if n%2 == 0:
            file.write("g0" + str(g) + ";p01;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][0] + ";" + "search;" + wall + ";" + str(cardList[lenght-7-n]) + ";\n")
            file.write("g0" + str(g) + ";p02;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][0] + ";" + "move;" + wall + ";" + str(cardList[lenght-7-n]) + ";\n")
            nb += 1
        else:
            file.write("g0" + str(g) + ";p01;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][0] + ";" + "move;" + wall + ";" + str(cardList[lenght-7-n]) + ";\n")
            file.write("g0" + str(g) + ";p02;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][0] + ";" + "search;" + wall + ";" + str(cardList[lenght-7-n]) + ";\n")
            nb += 1


    file.write("#pause;\n")
    for n in range(lenght, 2*lenght):
        if cardList[n] <20:
            wall = "L"
        elif cardList[n] < 40:
            wall = "B"
        else:
            wall = "R"

        if n-lenght < 6:
            training = "1"
        else:
            training = "0"
        if n%2 == 0:
            file.write("g0" + str(g) + ";p01;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][1] + ";" + "move;" + wall + ";" + str(cardList[n]) + ";\n")
            file.write("g0" + str(g) + ";p02;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][1] + ";" + "search;" + wall + ";" + str(cardList[n]) + ";\n")
            nb += 1
        else:
            file.write("g0" + str(g) + ";p01;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][1] + ";" + "search;" + wall + ";" + str(cardList[n]) + ";\n")
            file.write("g0" + str(g) + ";p02;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][1] + ";" + "move;" + wall + ";" + str(cardList[n]) + ";\n")
            nb += 1
    for n in range(lenght, 2*lenght-6):
        if cardList[lenght + 2*lenght-7-n] <20:
            wall = "L"
        elif cardList[lenght + 2*lenght-7-n] < 40:
            wall = "B"
        else:
            wall = "R"

        if n-lenght < 2*lenght - 6:
            training = "0"
            if n%2 == 0:
                file.write("g0" + str(g) + ";p01;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][1] + ";" + "search;" + wall + ";" + str(cardList[lenght + 2*lenght-7-n]) + ";\n")
                file.write("g0" + str(g) + ";p02;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][1] + ";" + "move;" + wall + ";" + str(cardList[lenght + 2*lenght-7-n]) + ";\n")
                nb += 1
            else:
                file.write("g0" + str(g) + ";p01;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][1] + ";" + "move;" + wall + ";" + str(cardList[lenght + 2*lenght-7-n]) + ";\n")
                file.write("g0" + str(g) + ";p02;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][1] + ";" + "search;" + wall + ";" + str(cardList[lenght + 2*lenght-7-n]) + ";\n")
                nb += 1

    file.write("#pause;\n")
    for n in range(2*lenght, 3*lenght):
        if cardList[n] <20:
            wall = "L"
        elif cardList[n] < 40:
            wall = "B"
        else:
            wall = "R"
            
        if n-2*lenght < 6:
            training = "1"
        else:
            training = "0"
        if n%2 == 0:
            file.write("g0" + str(g) + ";p01;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][2] + ";" + "move;" + wall + ";" + str(cardList[n]) + ";\n")
            file.write("g0" + str(g) + ";p02;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][2] + ";" + "search;" + wall + ";" + str(cardList[n]) + ";\n")
            nb += 1
        else:
            file.write("g0" + str(g) + ";p01;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][2] + ";" + "search;" + wall + ";" + str(cardList[n]) + ";\n")
            file.write("g0" + str(g) + ";p02;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][2] + ";" + "move;" + wall + ";" + str(cardList[n]) + ";\n")
            nb += 1
    for n in range(2*lenght, 3*lenght-6):
        if cardList[2*lenght + 3*lenght-7-n] <20:
            wall = "L"
        elif cardList[2*lenght + 3*lenght-7-n] < 40:
            wall = "B"
        else:
            wall = "R"
            
        if n-2*lenght < 3*lenght - 6:
            training = "0"
            if n%2 == 0:
                file.write("g0" + str(g) + ";p01;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][2] + ";" + "search;" + wall + ";" + str(cardList[2*lenght + 3*lenght-7-n]) + ";\n")
                file.write("g0" + str(g) + ";p02;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][2] + ";" + "move;" + wall + ";" + str(cardList[2*lenght + 3*lenght-7-n]) + ";\n")
                nb += 1
            else:
                file.write("g0" + str(g) + ";p01;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][2] + ";" + "move;" + wall + ";" + str(cardList[2*lenght + 3*lenght-7-n]) + ";\n")
                file.write("g0" + str(g) + ";p02;C;" + str(nb) + ";" + training + ";" + groupMove[g-1][2] + ";" + "search;" + wall + ";" + str(cardList[2*lenght + 3*lenght-7-n]) + ";\n")
                nb += 1
    
    if g != group:
        file.write("#pause;\n")
    else:
        file.write("#pause;")

file.close()
