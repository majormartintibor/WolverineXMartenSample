# WolverineXMartenSample

The scope of this repository is to show basic funcationalities of the critter stack (Marten and Wolverine only).

Business Scope:

The business scenario is a Promotion workflow. The complexity is kept low on purpose, only as much business logic
is defined as is minimal needed to show of critter stack features.
A promotion can be requested with for an employee with name Promotee = [name].
The promotion then has to be approved by the Supervisore, HR and finally CEO in this specific order.
If at any stage the promotion gets rejected the promotion is closed and rejected.
If it makes it through the approvals the promotione is closed as approved.
Two projections are defined on the read side:
1. PrmotionStatus is meant for the Promotee to check the Status
2. PromotionDetails is meant for elevated rights to see all releated information