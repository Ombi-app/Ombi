import { Component, OnInit } from "@angular/core";

import { IIssueCategory } from "../../interfaces";
import { IssuesService } from "../../services";

@Component({
    templateUrl: "./issues.component.html",
})
export class IssuesComponent implements OnInit {

    public categories: IIssueCategory[];
    public categoryToAdd: IIssueCategory = {id: 0, value: ""};

    constructor(private issuesService: IssuesService) {  }

    public ngOnInit() {
        this.getCategories();
    }    

    public addCategory(): void {
        this.issuesService.createCategory(this.categoryToAdd).subscribe(x => {
            if(x) {
                this.getCategories();
                this.categoryToAdd.value = "";
            }
        });
    }

    public deleteCategory(id: number) {
        this.issuesService.deleteCategory(id).subscribe(x => {
            if(x) {
                this.getCategories();
            }
        });
    }

    private getCategories() {
        this.issuesService.getCategories().subscribe(x => {
            this.categories = x;
        });
    }
}
