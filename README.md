# OM2_Lab
## Darbo rezultatai
X0 = (0.0, 0.0)
X1 = (1.0, 1.0)
Xm = (0.7, 0.7)

| Method            | Start Point | X1        | X2        | Volume    | Steps | GradCalls | ObjCalls | Total Calls |
|-------------------|-------------|-----------|-----------|-----------|-------|-----------|----------|-------------|
| GradientLipschitz | X0          |  0,000000 |  0,000000 |  0,000000 |     1 |         1 |        0 |           1 |
| SteepestDescent   | X0          |  0,000000 |  0,000000 |  0,000000 |     1 |         1 |        0 |           1 |
| DeformedSimplex   | X0          |  0,333334 |  0,333333 |  0,068041 |   119 |         - |      223 |         223 |
|-------------------|-------------|-----------|-----------|-----------|-------|-----------|----------|-------------|
| GradientLipschitz | X1          |  0,333334 |  0,333334 |  0,068041 |     6 |         6 |        0 |           6 |
| SteepestDescent   | X1          |  0,333333 |  0,333333 |  0,068041 |     2 |         2 |       34 |          36 |
| DeformedSimplex   | X1          |  0,333333 |  0,333333 |  0,068041 |    80 |         - |      157 |         157 |
|-------------------|-------------|-----------|-----------|-----------|-------|-----------|----------|-------------|
| GradientLipschitz | Xm          |  0,333333 |  0,333333 |  0,068041 |     6 |         6 |        0 |           6 |
| SteepestDescent   | Xm          |  0,333333 |  0,333333 |  0,068041 |     2 |         2 |       34 |          36 |
| DeformedSimplex   | Xm          |  0,333333 |  0,333333 |  0,068041 |    43 |         - |       84 |          84 |
|-------------------|-------------|-----------|-----------|-----------|-------|-----------|----------|-------------|

Vizualizacija X0 taškui:

![X0_optimizations](https://github.com/user-attachments/assets/6c24ce10-d2c4-4116-a2b9-7cda48a3b63d)

Vizualizacija X1 taškui:

![X1_optimizations](https://github.com/user-attachments/assets/110a4477-90e7-418e-80cb-0298fd65a1b8)

Vizualizacija Xm taškui:

![Xm_optimizations](https://github.com/user-attachments/assets/782f004a-db23-4f52-a013-d5960999d4d4)

