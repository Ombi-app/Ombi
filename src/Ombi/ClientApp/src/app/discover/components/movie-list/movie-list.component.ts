import { Component, OnInit, Input } from "@angular/core";
import { IDiscoverCardResult } from "../../interfaces";
import { RequestType} from "../../../interfaces";
import { SearchV2Service } from "../../../services";
import { ISearchTvResultV2 } from "../../../interfaces/ISearchTvResultV2";
import { ISearchMovieResultV2 } from "../../../interfaces/ISearchMovieResultV2";

@Component({
    selector: "movie-list",
    templateUrl: "./movie-list.component.html",
    styleUrls: ["./movie-list.component.scss"],
})
export class MovieListComponent  {

    public RequestType = RequestType;
    @Input() public result: IDiscoverCardResult[];

    public responsiveOptions: any;

    constructor() {
        this.responsiveOptions = [
            {
                breakpoint: '2559px',
                numVisible: 7,
                numScroll: 7
            },
            {
                breakpoint: '1024px',
                numVisible: 4,
                numScroll: 4
            },
            {
                breakpoint: '768px',
                numVisible: 2,
                numScroll: 2
            },
            {
                breakpoint: '560px',
                numVisible: 1,
                numScroll: 1
            }
        ];
    }


}
