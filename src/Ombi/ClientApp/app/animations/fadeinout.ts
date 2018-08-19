import { animate, style, transition, trigger } from "@angular/animations";
import { AnimationTriggerMetadata } from "@angular/animations";

export const fadeInOutAnimation: AnimationTriggerMetadata = trigger("fadeInOut", [
    transition(":enter", [   // :enter is alias to 'void => *'
        style({ opacity: 0 }),
        animate(1000, style({ opacity: 1 })),
    ]),
    transition(":leave", [   // :leave is alias to '* => void'
        animate(1000, style({ opacity: 0 })),
    ]),
]);
