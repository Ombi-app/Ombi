import { Component } from '@angular/core';
import { MenuItem } from 'primeng/components/common/api';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './app.component.html'
})
export class AppComponent {
    private items: MenuItem[];
    ngOnInit() {
        this.items = [
            {
                label: 'Ombi',
                routerLink: ['/'] 
            },
            {
                label: 'Search',
                routerLink: ['/search'] 
            },
        ];
    }
}