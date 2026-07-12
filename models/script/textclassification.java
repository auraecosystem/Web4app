package gemini.workshop;

import com.google.cloud.vertexai.api.Schema;
import com.google.cloud.vertexai.api.Type;
import dev.langchain4j.data.message.UserMessage;
import dev.langchain4j.memory.chat.MessageWindowChatMemory;
import dev.langchain4j.model.chat.ChatLanguageModel;
import dev.langchain4j.model.vertexai.VertexAiGeminiChatModel;
import dev.langchain4j.data.message.AiMessage;
import dev.langchain4j.service.AiServices;
import dev.langchain4j.service.SystemMessage;

import java.util.List;

public class TextClassification {

    enum Sentiment { POSITIVE, NEUTRAL, NEGATIVE }

    public static void main(String[] args) {
        ChatLanguageModel model = VertexAiGeminiChatModel.builder()
            .project(System.getenv("PROJECT_ID"))
            .location(System.getenv("LOCATION"))
            .modelName("gemini-2.0-flash")
            .maxOutputTokens(10)
            .maxRetries(3)
            .responseSchema(Schema.newBuilder()
                .setType(Type.STRING)
                .addAllEnum(List.of("POSITIVE", "NEUTRAL", "NEGATIVE"))
                .build())
            .build();


        interface SentimentAnalysis {
            @SystemMessage("""
                Analyze the sentiment of the text below.
                Respond only with one word to describe the sentiment.
                """)
            Sentiment analyze(String text);
        }

        MessageWindowChatMemory memory = MessageWindowChatMemory.withMaxMessages(10);
        memory.add(UserMessage.from("This is fantastic news!"));
        memory.add(AiMessage.from(Sentiment.POSITIVE.name()));

        memory.add(UserMessage.from("Pi is roughly equal to 3.14"));
        memory.add(AiMessage.from(Sentiment.NEUTRAL.name()));

        memory.add(UserMessage.from("I really disliked the pizza. Who would use pineapples as a pizza topping?"));
        memory.add(AiMessage.from(Sentiment.NEGATIVE.name()));

        SentimentAnalysis sentimentAnalysis =
            AiServices.builder(SentimentAnalysis.class)
                .chatLanguageModel(model)
                .chatMemory(memory)
                .build();

        System.out.println(sentimentAnalysis.analyze("I love strawberries!"));
    }
}
