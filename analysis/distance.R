library(readxl)
library(ARTool)
library(dplyr)
library(ggplot2)

#https://rcompanion.org/handbook/F_16.html
cat("\014")

# Load the data from Excel file using a absolute path
df <- read.csv("distance_traveled_statistics_all_pairs.csv")

# Check and prepare your data
head(df)
str(df)

# Normality Check Function
check_normality <- function(data, variable) {
  
  # Shapiro-Wilk Test
  shapiro_test <- shapiro.test(data[[variable]])
  shapiro_p <- shapiro_test$p.value
  
  return(shapiro_p)
}

# Convert columns to appropriate types
df$Condition <- factor(df$Condition)
df$Visualization <- factor(df$Visualization)
df$PairID <- factor(df$PairID)
df$Distance <- as.numeric(df$Distance)

# Perform the normality check for Distance
p_distance <- check_normality(df, 'Distance')
print(paste("Shapiro-Wilk Test p-value for Distance:", round(p_distance, 3)))

# Perform ART transformation
art_distance <- art(Distance ~ Condition * Visualization + Error(PairID/(Condition*Visualization)), data = df)

anova_result = anova(art_distance)

print(anova_result)

anova_result$part.eta.sq = with(anova_result, `Sum Sq`/(`Sum Sq` + `Sum Sq.res`))

print(anova_result$part.eta.sq)

