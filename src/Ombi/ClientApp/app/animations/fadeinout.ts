import { animate, style, transition, trigger } from "@angular/animations";
import { AnimationEntryMetadata } from "@angular/core";

export const fadeInOutAnimation: AnimationEntryMetadata = trigger("fadeInOut", [
    transition(":enter", [   // :enter is alias to 'void => *'
        style({ opacity: 0 }),
        animate(1000, style({ opacity: 1 })),
    ]),
    transition(":leave", [   // :leave is alias to '* => void'
        animate(1000, style({ opacity: 0 })),
    ]),
]);
