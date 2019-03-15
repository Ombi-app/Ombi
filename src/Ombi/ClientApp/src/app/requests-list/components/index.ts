import { RequestsListComponent } from "./requests-list.component";
import { MoviesGridComponent } from "./movies-grid/movies-grid.component";

import { RequestServiceV2 } from "../../services/requestV2.service";
import { RequestService } from "../../services";

export const components: any[] = [
    RequestsListComponent,
    MoviesGridComponent,
];


export const providers: any[] = [
    RequestService,
    RequestServiceV2,
];
