import { Component, Inject, Input, Output, EventEmitter } from "@angular/core";

@Component({
        standalone: false,
    selector: "media-poster",
    templateUrl: "./media-poster.component.html",
})
export class MediaPosterComponent {
    
    @Input() posterPath: string;
}
