import { Component, OnInit, ViewChild } from "@angular/core";

import { OverlayPanel } from "primeng/overlaypanel";
import { NotificationService, VoteService } from "../services";

import { IVoteEngineResult, IVoteViewModel, RequestTypes, VoteType } from "../interfaces";

@Component({
    templateUrl: "vote.component.html",
    styleUrls: ["vote.component.scss"],
})
export class VoteComponent implements OnInit {

    public showCurrent: boolean = true;
    public showCompleted: boolean;
    public viewModel: IVoteViewModel[];
    public currentVotes: IVoteViewModel[];
    public completedVotes: IVoteViewModel[];
    public VoteType = VoteType;
    public panelImage: string;
    @ViewChild("op") public overlayPanel: OverlayPanel;

    constructor(private voteService: VoteService, private notificationSerivce: NotificationService) { }

    public async ngOnInit() {
        this.viewModel = await this.voteService.getModel();
        this.filterLists();
     }

     public selectCurrentTab() {
        this.showCurrent = true;
        this.showCompleted = false;
    }
    
    public selectCompletedVotesTab() {
        this.showCurrent = false;
        this.showCompleted = true;
    }

     public toggle(event: any, image: string) {
        this.panelImage = image;
        this.overlayPanel.toggle(event);
     }

     public async upvote(vm: IVoteViewModel) {
        let result: IVoteEngineResult = {errorMessage:"", isError: false, message:"",result:false};
        switch(vm.requestType) {
            case RequestTypes.Album:
                result = await this.voteService.upvoteAlbum(vm.requestId);
                break;
            case RequestTypes.Movie:
                result = await this.voteService.upvoteMovie(vm.requestId);
                break;
            case RequestTypes.TvShow:
                result = await this.voteService.upvoteTv(vm.requestId);
                break;
        }

        if(result.isError) {
            this.notificationSerivce.error(result.errorMessage);
        } else {
            this.notificationSerivce.success("Voted!");
            vm.alreadyVoted = true;
            vm.myVote = VoteType.Upvote;
            this.filterLists();
        }
    }

    public async downvote(vm: IVoteViewModel) {
        let result: IVoteEngineResult = {errorMessage:"", isError: false, message:"",result:false};
        switch(vm.requestType) {
            case RequestTypes.Album:
                result = await this.voteService.downvoteAlbum(vm.requestId);
                break;
            case RequestTypes.Movie:
                result = await this.voteService.downvoteMovie(vm.requestId);
                break;
            case RequestTypes.TvShow:
                result = await this.voteService.downvoteTv(vm.requestId);
                break;
        }

        if(result.isError) {
            this.notificationSerivce.error(result.errorMessage);
        } else {
            this.notificationSerivce.success("Voted!"); 
            vm.alreadyVoted = true;
            vm.myVote = VoteType.Downvote;
            this.filterLists();
        }
    }

    private filterLists() {
        this.completedVotes = this.viewModel.filter(vm => {
            return vm.alreadyVoted;
        });
        this.currentVotes = this.viewModel.filter(vm => {
            return !vm.alreadyVoted;
        });
    }
}
