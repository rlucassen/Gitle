alter table Issue
add EstimatePublic bit;

update Issue set EstimatePublic = 1;