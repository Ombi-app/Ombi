import { RequestsListComponent } from "./requests-list.component";
import { MoviesGridComponent } from "./movies-grid/movies-grid.component";

import { RequestServiceV2 } from "../../services/requestV2.service";
import { RequestService } from "../../services";
import { TvGridComponent } from "./tv-grid/tv-grid.component";
import { GridSpinnerComponent } from "./grid-spinner/grid-spinner.component";
import { RequestOptionsComponent } from "./options/request-options.component";
import { AlbumsGridComponent } from "./albums-grid/albums-grid.component";

export const components: any[] = [
    RequestsListComponent,
    MoviesGridComponent,
    TvGridComponent,
    GridSpinnerComponent,
    RequestOptionsComponent,
    AlbumsGridComponent
];

export const entryComponents: any[] = [
    RequestOptionsComponent
];

export const providers: any[] = [
    RequestService,
    RequestServiceV2,
];
